"""Portable source-contract checks, NOT a replacement for Windows compilation/runtime tests."""
import re
import unittest
import xml.etree.ElementTree as ET
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SITE = ROOT / 'OnlineGroceryStore'

class SourceContracts(unittest.TestCase):
    def test_configuration_is_well_formed_and_hardened(self):
        for path in SITE.rglob('Web.config'):
            ET.parse(path)
        config = ET.parse(SITE / 'Web.config').getroot()
        self.assertEqual(config.find('./system.web/compilation').get('targetFramework'), '4.8')
        self.assertEqual(config.find('./system.web/httpCookies').get('requireSSL'), 'true')
        self.assertEqual(config.find('./system.web/customErrors').get('mode'), 'On')
        self.assertEqual(config.find('./system.web/pages').get('enableViewStateMac'), 'true')

    def test_required_pages_and_helpers_exist(self):
        for name in ['Default','Register','Login','Logout','Products','ProductDetails','Cart','Checkout','Orders','OrderDetails','About','Contact']:
            self.assertTrue((SITE / (name + '.aspx')).is_file(), name)
        for name in ['Dashboard','ManageCustomers','ManageCategories','ManageProducts','AddProduct','EditProduct','ManageOrders','Reports']:
            self.assertTrue((SITE / 'Admin' / (name + '.aspx')).is_file(), name)
        for name in ['DatabaseHelper','SecurityHelper','CommonHelper','StoreService','AdminService','GroceryPage']:
            self.assertTrue((SITE / 'App_Code' / (name + '.vb')).is_file(), name)

    def test_page_wiring_and_event_handlers(self):
        for path in SITE.rglob('*.aspx'):
            markup = path.read_text()
            directive = re.search(r'<%@ Page\s+(.*?)%>', markup, re.S).group(1)
            attributes = dict(re.findall(r'(\w+)="([^"]*)"', directive))
            if 'CodeFile' not in attributes:
                self.assertEqual(path.name, 'Error.aspx')
                continue
            code = (path.parent / attributes['CodeFile']).read_text()
            self.assertIn('Class ' + attributes['Inherits'], code, str(path))
            master = SITE / attributes['MasterPageFile'].removeprefix('~/')
            self.assertTrue(master.is_file(), str(master))
            for handler in re.findall(r'\bOn(?:Click|RowCommand)="([^"]+)"', markup):
                self.assertRegex(code, r'\bSub\s+' + handler + r'\s*\(', str(path))
            if path.parent.name == 'Admin':
                self.assertIn('Inherits AdminPage', code)
            if path.stem in ['Cart','Checkout','Orders','OrderDetails','Logout']:
                self.assertIn('Inherits CustomerPage', code)

    def test_validators_reference_existing_controls(self):
        for path in SITE.rglob('*.aspx'):
            markup = path.read_text()
            ids = re.findall(r'\bID="([^"]+)"', markup)
            self.assertEqual(len(ids), len(set(ids)), str(path))
            for target in re.findall(r'(?:ControlToValidate|ControlToCompare|AssociatedControlID)="([^"]+)"', markup):
                self.assertIn(target, ids, str(path))

    def test_schema_has_keys_and_relationships(self):
        sql = (ROOT / 'database/schema.sql').read_text()
        tables = re.findall(r'CREATE TABLE \[(\w+)\]', sql)
        self.assertEqual(set(tables), {'Users','Categories','Products','Cart','Orders','OrderItems'})
        self.assertEqual(sql.count('PRIMARY KEY'), 6)
        self.assertEqual(sql.count('FOREIGN KEY'), 6)
        self.assertIn('UX_Users_Email ON [Users] ([Email])', sql)
        self.assertIn('UX_Cart_User_Product ON [Cart] ([UserID], [ProductID])', sql)
        self.assertIn('UX_Orders_Token ON [Orders] ([CheckoutToken])', sql)
        self.assertIn('[PasswordHash] TEXT(255)', sql)
        self.assertNotIn('[Password]', sql)
        self.assertNotIn('CASCADE', sql)

    def test_literal_sql_placeholder_counts(self):
        # Check positional OleDb argument counts for calls with literal SQL and explicit params.
        checked = 0
        for path in (SITE / 'App_Code').glob('*.vb'):
            source = path.read_text()
            pattern = r'(?:ExecuteNonQuery|ExecuteScalar|FillDataTable|Execute|Scalar|Table)\("((?:[^"]|"")*)"'
            for match in re.finditer(pattern, source):
                sql = match.group(1)
                pos = match.end()
                depth = 1
                quoted = False
                args = 0
                tail_start = pos
                while pos < len(source) and depth:
                    c = source[pos]
                    if c == '"':
                        if quoted and pos + 1 < len(source) and source[pos+1] == '"':
                            pos += 2
                            continue
                        quoted = not quoted
                    elif not quoted:
                        if c == '(':
                            depth += 1
                        elif c == ')':
                            depth -= 1
                        elif c == ',' and depth == 1:
                            args += 1
                    pos += 1
                tail = source[tail_start:pos]
                if 'args.ToArray()' in tail:
                    continue  # Dynamic product editor parameter list is reviewed separately.
                self.assertEqual(sql.count('?'), args, f'{path.name}: {sql}')
                checked += 1
        self.assertGreater(checked, 40)

    def test_checkout_and_cancellation_contracts(self):
        source = (SITE / 'App_Code/StoreService.vb').read_text()
        checkout = source.split('Public Shared Function Checkout(')[1].split('Public Shared Function Orders(')[0]
        for token in ['New DbSession(True)', 'LockCart(db, userId)', '[CheckoutToken]=? AND [UserID]=?', 'ValidateStock', '[StockQuantity]>=?', 'db.Identity()', 'INSERT INTO [OrderItems]', 'DELETE FROM [Cart]', 'db.Commit()']:
            self.assertIn(token, checkout)
        self.assertLess(checkout.index('INSERT INTO [OrderItems]'), checkout.index('DELETE FROM [Cart]'))
        admin = (SITE / 'App_Code/AdminService.vb').read_text()
        self.assertIn('WHERE [OrderID]=? AND [Status]=?', admin)
        self.assertIn('SET [StockQuantity]=[StockQuantity]+?, [Version]=[Version]+1', admin)
        self.assertIn('WHERE [ProductID]=? AND [Version]=?', admin)

    def test_security_contracts(self):
        security = (SITE / 'App_Code/SecurityHelper.vb').read_text()
        self.assertIn('Iterations As Integer = 600000', security)
        self.assertIn('HashAlgorithmName.SHA256', security)
        self.assertIn('rng.GetBytes(salt)', security)
        self.assertIn('difference Or', security)
        self.assertIn('LoginThrottle.Check', security)
        base = (SITE / 'App_Code/GroceryPage.vb').read_text()
        self.assertIn('Request.Form("__csrf") <> CsrfToken', base)
        self.assertIn('ViewStateUserKey = CsrfToken', base)
        self.assertIn('identity.Ticket.UserData <>', base)
        self.assertIn('CStr(rows.Rows(0)("Status")) <> "Active"', base)
        for path in [SITE / 'Site.Master', SITE / 'Admin/AdminMaster.master']:
            self.assertIn('name="__csrf"', path.read_text())
        for method in ['Order(', 'OrderItems(']:
            part = (SITE / 'App_Code/StoreService.vb').read_text().split('Public Shared Function ' + method)[1]
            self.assertIn('AND o.[UserID]=?', part.split('End Function')[0])

    def test_local_links_and_assets_resolve(self):
        for path in list(SITE.rglob('*.aspx')) + list(SITE.rglob('*.master')):
            markup = path.read_text()
            for url in re.findall(r'(?:href|src|NavigateUrl)="([^"<>]+)"', markup):
                if url.startswith(('https:', '#')) or '{' in url:
                    continue
                clean = url.split('?')[0]
                target = SITE / clean[2:] if clean.startswith('~/') else path.parent / clean
                self.assertTrue(target.is_file(), f'{path}: {url}')
        ET.parse(SITE / 'Content/grocery.svg')

    def test_no_database_or_credentials_checked_in(self):
        self.assertFalse(list(SITE.rglob('*.accdb')))
        self.assertFalse(list(SITE.rglob('*.laccdb')))
        self.assertIn('*.accdb', (ROOT / '.gitignore').read_text())
        setup = (ROOT / 'scripts/Initialize-Database.ps1').read_text()
        self.assertIn('-AsSecureString', setup)
        self.assertIn('Database already exists', setup)

if __name__ == '__main__':
    unittest.main(verbosity=2)
