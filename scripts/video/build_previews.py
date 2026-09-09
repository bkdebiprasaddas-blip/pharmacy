"""Render ASPX markup as static HTML with explicit sample data for the video.
This is NOT an ASP.NET runtime or a database-backed substitute for the application.
"""
from pathlib import Path
import re, html, json, base64
from bs4 import BeautifulSoup

ROOT = Path(__file__).resolve().parents[2]
SITE = ROOT / 'OnlineGroceryStore'
OUT = ROOT / '.video-work'
OUT.mkdir(exist_ok=True)
CSS = (SITE / 'Content/site.css').read_text()
IMAGE = 'data:image/svg+xml;base64,' + base64.b64encode((SITE / 'Content/grocery.svg').read_bytes()).decode()
E = lambda x: html.escape(str(x), quote=True)
M = lambda x: '₹' + f'{float(x):,.2f}'
CATS = [dict(CategoryID=i+1, CategoryName=n, Description=n+' and everyday essentials', Status='Active') for i,n in enumerate(['Fruits','Vegetables','Dairy','Beverages','Snacks','Rice & Grains','Pulses','Bakery','Personal Care','Household Items'])]
PRODUCTS = [dict(ProductID=i+1,CategoryID=c,CategoryName=CATS[c-1]['CategoryName'],ProductName=n,Brand=b,Unit=u,Price=p,StockQuantity=s,Status='Active',ExpiryDate='—') for i,(c,n,b,u,p,s) in enumerate([
(1,'Fresh apples','Orchard Picks','1 kg',180,40),(1,'Bananas','Orchard Picks','6 pieces',45,60),(2,'Tomatoes','Farm Selection','1 kg',35,50),(2,'Potatoes','Farm Selection','1 kg',30,70),(3,'Toned milk','Daily Dairy','500 ml',28,30),(4,'Orange juice','Citrus House','1 litre',110,15),(5,'Roasted makhana','Pantry Picks','100 g',95,4),(6,'Basmati rice','Harvest Pantry','1 kg',120,45),(7,'Toor dal','Harvest Pantry','1 kg',145,25),(8,'Whole wheat bread','Morning Bake','400 g',45,3),(9,'Hand soap','Home Basics','100 g',35,20),(10,'Dishwashing liquid','Home Basics','500 ml',90,18)])]
CART=[dict(PRODUCTS[0],CartID=1,Quantity=2,Subtotal=360),dict(PRODUCTS[7],CartID=2,Quantity=1,Subtotal=120)]
ORDER=dict(OrderID=1003,FullName='Aarav Shah',Email='aarav@example.test',Phone='Demo contact',OrderDate='09 Sep 2026 12:15',TotalAmount=480,PaymentMethod='Cash on Delivery',Status='Pending',DeliveryAddress='24 Sample Street',City='Ahmedabad')
ORDERS=[ORDER,dict(ORDER,OrderID=1002,OrderDate='08 Sep 2026 16:20',TotalAmount=270,Status='Packed'),dict(ORDER,OrderID=1001,OrderDate='06 Sep 2026 10:05',TotalAmount=920,Status='Delivered')]
CUSTOMERS=[dict(UserID=2,FullName='Aarav Shah',Email='aarav@example.test',Phone='Demo contact',Address='24 Sample Street',City='Ahmedabad',Status='Active'),dict(UserID=3,FullName='Mira Patel',Email='mira@example.test',Phone='Demo contact',Address='8 Demo Road',City='Ahmedabad',Status='Active'),dict(UserID=4,FullName='Rohan Mehta',Email='rohan@example.test',Phone='Demo contact',Address='15 Example Lane',City='Ahmedabad',Status='Inactive')]

def bind(text,row):
    def expression(m):
        exp=m.group(1)
        if 'CommonHelper.ProductImage' in exp: return IMAGE
        if 'ResolveUrl' in exp or '.aspx?' in exp: return '#'
        field=re.search(r'Eval\("(\w+)"\)',exp)
        if not field: return ''
        value=row.get(field.group(1),'')
        if 'FormatCurrency' in exp: value=M(value)
        return E(value)
    return re.sub(r'<%#:?\s*(.*?)\s*%>', expression, text, flags=re.S)

def convert(markup,data,values):
    # Expand repeaters from the real templates before HTML parsing.
    def repeater(m):
        ident=re.search(r'ID="([^"]+)"',m.group(1),re.I).group(1)
        body=m.group(2)
        def template(name):
            q=re.search(f'<{name}>(.*?)</{name}>',body,re.I|re.S)
            return q.group(1) if q else ''
        return template('HeaderTemplate')+''.join(bind(template('ItemTemplate'),row) for row in data.get(ident,[]))+template('FooterTemplate')
    markup=re.sub(r'<asp:Repeater\b([^>]*)>(.*?)</asp:Repeater>',repeater,markup,flags=re.I|re.S)
    markup=re.sub(r'<%@.*?%>','',markup,flags=re.S)
    soup=BeautifulSoup(markup,'html.parser')
    for grid in list(soup.find_all(['asp:gridview','asp:detailsview'])):
        rows=data.get(grid.get('id'),[])
        fields=grid.find_all(['asp:boundfield','asp:hyperlinkfield','asp:templatefield'])
        def cell(f,row):
            if f.name=='asp:boundfield':
                v=row.get(f.get('datafield'),'')
                if ':C2' in f.get('dataformatstring','') and v!='': v=M(v)
                return E(v)
            if f.name=='asp:hyperlinkfield': return '<a href="#">'+E(f.get('text','View'))+'</a>'
            return convert(bind(html.unescape(str(f.find('itemtemplate'))),row),{},values)
        if grid.name=='asp:detailsview':
            table=''.join('<tr><td>'+E(f.get('headertext',''))+'</td><td>'+cell(f,rows[0])+'</td></tr>' for f in fields) if rows else ''
        else:
            table='<thead><tr>'+''.join('<th>'+E(f.get('headertext',''))+'</th>' for f in fields)+'</tr></thead><tbody>'
            table+=''.join('<tr>'+''.join('<td>'+cell(f,row)+'</td>' for f in fields)+'</tr>' for row in rows)+'</tbody>'
        result='<table id="'+grid.get('id','')+'" class="'+grid.get('cssclass','table')+'">'+table+'</table>'
        grid.replace_with(BeautifulSoup(result,'html.parser'))
    for tag in list(soup.find_all(True)):
        if tag.parent is None: continue
        name=tag.name
        if name in ['columns','fields','itemtemplate','asp:content']:
            tag.unwrap();continue
        if not name.startswith('asp:'): continue
        if 'validator' in name or name=='asp:validationsummary': tag.decompose();continue
        attrs=dict(tag.attrs)
        ident=attrs.get('id','')
        text=values.get(ident,attrs.get('text',''))
        cls=attrs.get('cssclass','')
        out={'id':ident,'class':cls}
        if name in ['asp:label','asp:literal']:
            tag.name='label' if name=='asp:label' and 'associatedcontrolid' in attrs else 'span'
            tag.clear();tag.append(str(text))
            if 'associatedcontrolid' in attrs: out['for']=attrs['associatedcontrolid']
        elif name=='asp:textbox':
            mode=attrs.get('textmode','').lower()
            if mode=='multiline':tag.name='textarea';tag.clear();tag.append(str(text))
            else:tag.name='input';out.update(type=mode if mode in ['password','number','date','email'] else 'text',value=str(text))
        elif name in ['asp:button','asp:linkbutton','asp:hyperlink']:
            tag.name='button' if name=='asp:button' else 'a';tag.clear();tag.append(str(text));out['type' if tag.name=='button' else 'href']='button' if tag.name=='button' else '#'
        elif name=='asp:dropdownlist':
            options=values.get(ident+'Options')
            if options:
                tag.clear()
                for option in options:
                    opt=soup.new_tag('option');opt.string=option;tag.append(opt)
            else:
                for item in tag.find_all('asp:listitem'): item.name='option';item.attrs={}
            tag.name='select'
            for opt in tag.find_all('option'):
                if opt.get_text()==text: opt['selected']='selected'
        elif name=='asp:listitem':
            tag.name='option'
        elif name=='asp:image': tag.name='img';out.update(src=IMAGE,alt='Grocery illustration')
        elif name=='asp:panel': tag.name='div'
        else: tag.name='span'
        tag.attrs={k:v for k,v in out.items() if v!=''}
    return str(soup).replace('src="Content/grocery.svg"','src="'+IMAGE+'"')

NAV=['Home','Products','Cart','My orders','About','Contact']
ADMINNAV=['Dashboard','Customers','Categories','Products','Orders','Reports']
def document(page,data=None,values=None,logged=True,notice=''):
    admin=page.startswith('Admin/')
    body=convert((SITE/page).read_text(),data or {},values or {})
    brand='<a class="brand"><span class="brand-icon">✦</span> fresh<span>basket</span><small>'+('STORE MANAGEMENT' if admin else 'EVERYDAY GOODNESS')+'</small></a>'
    nav='<nav>'+''.join('<a>'+n+'</a>' for n in NAV)+'</nav>'
    account='<span>Store Administrator</span><a>View store ↗</a><a>Log out</a>' if admin else ('<span>Hi, Aarav Shah</span><a>Log out</a>' if logged else '<a>Log in</a><a class="btn btn-primary">Sign up</a>')
    head='<header class="site-header"><div class="header-inner">'+brand+('' if admin else nav)+'<div class="account-nav">'+account+'</div></div></header>'
    if notice: body='<div class="notice">'+E(notice)+'</div>'+body
    if admin:
        body=head+'<div class="admin-shell"><aside><span class="eyebrow">WORKSPACE</span><nav>'+''.join('<a>'+n+'</a>' for n in ADMINNAV)+'</nav><p class="muted">FreshBasket<br>Administrator area</p></aside><main class="admin-main">'+body+'</main></div>'
    else:
        body='<div class="announcement">Fresh essentials. Thoughtful shopping. <span>Cash on Delivery available</span></div>'+head+'<main class="container store-main">'+body+'</main>'
    return '<!doctype html><html lang="en"><meta charset="utf-8"><style>'+CSS+'\n::-webkit-scrollbar{width:5px}::-webkit-scrollbar-thumb{background:#ccd8c4} .video-highlight{outline:3px solid #99ae42!important;outline-offset:6px;box-shadow:0 0 0 11px #a5bc4520!important} button{font-family:inherit} </style><body>'+body+'</body></html>'

SCENES=[]
def scene(page,title,caption,seconds=7,data=None,values=None,scroll=0,focus='',logged=True,notice='',chapter='CUSTOMER EXPERIENCE'):
    index=len(SCENES)
    filename=f'preview-{index:02}.html'
    (OUT/filename).write_text(document(page,data,values,logged,notice))
    SCENES.append(dict(file=filename,page=page,title=title,caption=caption,seconds=seconds,scroll=scroll,focus=focus,chapter=chapter))

scene('Default.aspx','FreshBasket · UI & feature tour','Source-based UI previews with illustrative data—not a recording of the running Windows/Access application.',7,data={'CategoryList':CATS,'ProductList':PRODUCTS[:8]},logged=False,chapter='ONLINE GROCERY STORE MANAGEMENT SYSTEM')
scene('Default.aspx','01 / Discover the storefront','A welcoming home page brings grocery categories, featured products, and customer navigation together.',6,data={'CategoryList':CATS,'ProductList':PRODUCTS[:8]},scroll=430,focus='.category-grid',logged=False)
scene('Register.aspx','02 / Create a customer account','Registration collects contact and delivery details. The source includes validation, unique emails, and hashed passwords.',7,values={'FullName':'Aarav Shah','Email':'aarav@example.test','Phone':'9876543210','City':'Ahmedabad','Address':'24 Sample Street','Password':'IllustrationOnly','ConfirmPassword':'IllustrationOnly'},scroll=120,logged=False,focus='#SaveButton')
scene('Login.aspx','03 / Log in to your workspace','Customers enter the store; administrators enter the dashboard. Session and role checks protect restricted pages.',6,values={'Email':'aarav@example.test','Password':'IllustrationOnly'},logged=False,focus='.form-panel')
scene('Products.aspx','04 / Search and filter groceries','Browse the catalog, search product names or brands, and narrow the results by category.',7,data={'ProductList':PRODUCTS[:8]},values={'CategoryOptions':['All categories']+[c['CategoryName'] for c in CATS]},focus='.filter-bar')
scene('Products.aspx','Find the right pantry essential','Illustrated search: “rice” in Rice & Grains. The matching product shows its pack size, price, and available stock.',6,data={'ProductList':[PRODUCTS[7]]},values={'Search':'rice','Category':'Rice & Grains','CategoryOptions':['All categories']+[c['CategoryName'] for c in CATS]},focus='.product-card')
scene('ProductDetails.aspx','05 / Choose a product and quantity','Product details bring price, unit, and availability into view. Add-to-cart logic checks the requested quantity against stock.',7,values={'ProductName':'Fresh apples','CategoryName':'Fruits','BrandUnit':'Orchard Picks · 1 kg','PriceLabel':'₹180.00','Description':'Sample grocery product for demonstration.','Stock':'40 units available','Quantity':'2'},scroll=90,focus='#SaveButton')
scene('Cart.aspx','06 / Review and update the basket','This sample basket totals ₹480. Repeated additions merge into one product row; quantities, removal, and clearing are supported.',8,data={'CartGrid':CART},values={'TotalLabel':'₹480.00'},scroll=100,focus='.cart-summary')
scene('Checkout.aspx','07 / Confirm delivery and payment','Enter the delivery address and choose Cash on Delivery. No payment gateway or online payment collection is included.',8,data={'ItemsGrid':CART},values={'TotalLabel':'₹480.00','Address':'24 Sample Street','City':'Ahmedabad'},scroll=390,focus='#Payment')
scene('OrderDetails.aspx','08 / View an order confirmation','Illustrative order #1003 is Pending. Checkout source uses a transaction to create items, reduce stock, and clear the cart.',8,data={'OrderInfo':[ORDER],'ItemsGrid':CART},scroll=135,notice='Illustrative confirmation: order placed successfully.',focus='#OrderInfo')
scene('Orders.aspx','09 / Follow your order history','Customers can review their own orders and open details. Purchased names, units, and prices are preserved in order items.',7,data={'OrdersGrid':ORDERS},focus='#OrdersGrid')
scene('Admin/Dashboard.aspx','10 / Get a store-wide overview','The admin dashboard displays customer, product, and order counts, delivered revenue, recent orders, and low-stock alerts.',8,data={'Stats':[{'Label':'Customers','Value':'3'},{'Label':'Products','Value':'12'},{'Label':'Orders','Value':'3'},{'Label':'Delivered revenue','Value':'₹920.00'},{'Label':'Pending orders','Value':'1'}],'RecentOrders':ORDERS,'LowStockGrid':[PRODUCTS[9],PRODUCTS[6]],'TopProducts':[]},focus='.stats-grid',chapter='ADMINISTRATOR WORKSPACE')
scene('Admin/ManageCategories.aspx','11 / Organize grocery categories','Add, edit, or deactivate categories. Categories containing products cannot be deleted; deactivation preserves the records.',7,data={'CategoriesGrid':CATS[:5]},chapter='ADMINISTRATOR WORKSPACE',scroll=370,focus='#SaveButton')
scene('Admin/ManageProducts.aspx','12 / Manage products and inventory','Search the catalog, edit stock and product details, or add new items. Referenced products are protected from deletion.',7,data={'ProductsGrid':PRODUCTS[:8]},chapter='ADMINISTRATOR WORKSPACE',focus='#ProductsGrid')
scene('Admin/EditProduct.aspx','Edit a product with clear controls','The editor includes category, unit, price, stock, expiry, and status. Version checks guard against overwriting concurrent stock changes.',7,values={'ProductName':'Fresh apples','Category':'Fruits','CategoryOptions':[c['CategoryName'] for c in CATS],'Brand':'Orchard Picks','Unit':'1 kg','Price':'180.00','Stock':'38','Status':'Active','Description':'Sample grocery product for demonstration.'},scroll=180,focus='#Stock',chapter='ADMINISTRATOR WORKSPACE')
scene('Admin/ManageCustomers.aspx','13 / Manage customer access','Search customer records and activate or deactivate accounts. Passwords are never displayed in the administrator interface.',7,data={'CustomersGrid':CUSTOMERS},focus='#CustomersGrid',chapter='ADMINISTRATOR WORKSPACE')
scene('Admin/ManageOrders.aspx','14 / Move orders through fulfillment','Orders follow Pending → Confirmed → Packed → Out for Delivery → Delivered. Cancellation before dispatch restores inventory once.',9,data={'OrdersGrid':ORDERS,'ItemsGrid':CART},values={'ItemsHeading':'Items for order #1003','OrderIdInput':'1003','NextStatus':'Confirmed'},scroll=650,focus='#NextStatus',chapter='ADMINISTRATOR WORKSPACE')
scene('Admin/Reports.aspx','15 / Review sales and stock reports','Filter orders by date and choose a low-stock threshold. Revenue counts delivered orders only; reports can be printed to PDF.',8,data={'SalesGrid':ORDERS,'LowStockGrid':[PRODUCTS[9],PRODUCTS[6]],'ProductsGrid':PRODUCTS[:5]},values={'StartDate':'2026-09-01','EndDate':'2026-09-09','Threshold':'5','Revenue':'₹920.00'},focus='.filter-bar',chapter='ADMINISTRATOR WORKSPACE')
scene('Default.aspx','Ready for a Windows demonstration','Next: create GroceryDB.accdb, open the Web Site in Visual Studio, enable HTTPS, and run the Windows tests before a live demo.',8,data={'CategoryList':CATS,'ProductList':PRODUCTS[:8]},logged=False,chapter='ASP.NET WEB FORMS · VB.NET · MICROSOFT ACCESS')
for index, end in {2:320, 8:540, 9:475, 11:350, 17:270}.items():
    SCENES[index]['scrollEnd']=end
(OUT/'scenes.json').write_text(json.dumps(SCENES,indent=2,ensure_ascii=False))
print(f'Created {len(SCENES)} source-based preview scenes, {sum(s["seconds"] for s in SCENES)} seconds.')
