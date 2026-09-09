# Run with Windows PowerShell 5.1 and a matching-bitness Microsoft ACE provider.
[CmdletBinding()]
param(
    [string]$DatabasePath = (Join-Path $PSScriptRoot '..\OnlineGroceryStore\App_Data\GroceryDB.accdb'),
    [string]$AdminEmail,
    [string]$AdminName = 'Store Administrator',
    [Security.SecureString]$AdminPassword,
    [switch]$SkipSampleProducts
)
$ErrorActionPreference = 'Stop'
if ($PSVersionTable.PSEdition -ne 'Desktop') { throw 'Use Windows PowerShell 5.1, not PowerShell 7.' }
if (Test-Path $DatabasePath) { throw 'Database already exists. Back it up and choose a NEW path; this script never overwrites data.' }
if (-not $AdminEmail) { $AdminEmail = Read-Host 'Administrator email' }
$AdminEmail = $AdminEmail.Trim().ToLowerInvariant()
if ($AdminEmail.Length -gt 254 -or $AdminEmail -notmatch '^[^\s@]+@[^\s@]+\.[^\s@]+$') { throw 'Invalid administrator email.' }
if ([string]::IsNullOrWhiteSpace($AdminName) -or $AdminName.Length -gt 100) { throw 'Admin name must be 1-100 characters.' }
if (-not $AdminPassword) { $AdminPassword = Read-Host 'Administrator password (12-128 characters)' -AsSecureString }
$bstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($AdminPassword)
try { $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr) }
if ($plain.Length -lt 12 -or $plain.Length -gt 128) { throw 'Password must be 12-128 characters.' }
$salt = New-Object byte[] 16
$rng = [Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($salt)
$rng.Dispose()
$kdf = [Security.Cryptography.Rfc2898DeriveBytes]::new($plain, $salt, 600000, [Security.Cryptography.HashAlgorithmName]::SHA256)
try { $hash = 'pbkdf2-sha256$600000$' + [Convert]::ToBase64String($salt) + '$' + [Convert]::ToBase64String($kdf.GetBytes(32)) }
finally { $kdf.Dispose(); $plain = $null }
$DatabasePath = [IO.Path]::GetFullPath($DatabasePath)
$null = New-Item -ItemType Directory -Force -Path (Split-Path $DatabasePath)
$connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=$DatabasePath;Persist Security Info=False;"
$catalog = $null
$connection = $null
$created = $false
try {
    $catalog = New-Object -ComObject ADOX.Catalog
    $catalog.Create($connectionString)
    $created = $true
    $catalog.ActiveConnection.Close()
    [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($catalog)
    $catalog = $null
    $connection = [Data.OleDb.OleDbConnection]::new($connectionString)
    $connection.Open()
    function Invoke-Sql([string]$Sql, [object[]]$Values = @()) {
        $cmd = $connection.CreateCommand()
        try {
            $cmd.CommandText = $Sql
            foreach ($value in $Values) {
                $type = [Data.OleDb.OleDbType]::VarWChar
                if ($value -is [int]) { $type = [Data.OleDb.OleDbType]::Integer }
                elseif ($value -is [decimal]) { $type = [Data.OleDb.OleDbType]::Currency }
                elseif ($value -is [datetime]) { $type = [Data.OleDb.OleDbType]::Date }
                $p = $cmd.Parameters.Add('?', $type)
                $p.Value = $value
            }
            [void]$cmd.ExecuteNonQuery()
        } finally { $cmd.Dispose() }
    }
    $schema = Get-Content (Join-Path $PSScriptRoot '..\database\schema.sql') -Raw
    foreach ($statement in $schema.Split(';')) {
        if ($statement.Trim()) { Invoke-Sql $statement.Trim() }
    }
    Invoke-Sql "INSERT INTO [Users] ([FullName],[Email],[PasswordHash],[Phone],[Address],[City],[Role],[Status],[CreatedDate],[CartVersion]) VALUES (?,?,?,'Not set','Not set','Not set','Admin','Active',?,0)" @($AdminName.Trim(), $AdminEmail, $hash, [DateTime]::Now)
    $categories = @('Fruits','Vegetables','Dairy','Beverages','Snacks','Rice & Grains','Pulses','Bakery','Personal Care','Household Items')
    foreach ($category in $categories) {
        Invoke-Sql "INSERT INTO [Categories] ([CategoryName],[Description],[Status]) VALUES (?,?,'Active')" @($category, "$category and everyday essentials")
    }
    if (-not $SkipSampleProducts) {
        # Demo products/prices, not real-time store or brand information.
        $products = @(
            @(1,'Fresh apples','Orchard Picks','1 kg',[decimal]180,40),
            @(1,'Bananas','Orchard Picks','6 pieces',[decimal]45,60),
            @(2,'Tomatoes','Farm Selection','1 kg',[decimal]35,50),
            @(2,'Potatoes','Farm Selection','1 kg',[decimal]30,70),
            @(3,'Toned milk','Daily Dairy','500 ml',[decimal]28,30),
            @(4,'Orange juice','Citrus House','1 litre',[decimal]110,15),
            @(5,'Roasted makhana','Pantry Picks','100 g',[decimal]95,4),
            @(6,'Basmati rice','Harvest Pantry','1 kg',[decimal]120,45),
            @(7,'Toor dal','Harvest Pantry','1 kg',[decimal]145,25),
            @(8,'Whole wheat bread','Morning Bake','400 g',[decimal]45,3),
            @(9,'Hand soap','Home Basics','100 g',[decimal]35,20),
            @(10,'Dishwashing liquid','Home Basics','500 ml',[decimal]90,18)
        )
        foreach ($p in $products) {
            Invoke-Sql "INSERT INTO [Products] ([CategoryID],[ProductName],[Brand],[Unit],[Price],[StockQuantity],[Description],[Status],[CreatedDate],[Version]) VALUES (?,?,?,?,?,?,?,'Active',?,0)" @([int]$p[0],$p[1],$p[2],$p[3],[decimal]$p[4],[int]$p[5], 'Sample grocery product for demonstration. Set expiry dates and verify pricing before real use.', [DateTime]::Now)
        }
    }
    $connection.Close()
    # Enforce basic field-level business rules even for direct Access edits.
    $catalog = New-Object -ComObject ADOX.Catalog
    $catalog.ActiveConnection = $connectionString
    $rules = @(
        @('Users','Role',"In ('Customer','Admin')"),
        @('Users','Status',"In ('Active','Inactive')"),
        @('Categories','Status',"In ('Active','Inactive')"),
        @('Products','Price','>=0'),
        @('Products','StockQuantity','>=0'),
        @('Products','Version','>=0'),
        @('Products','Status',"In ('Active','Inactive')"),
        @('Cart','Quantity','>0'),
        @('Orders','TotalAmount','>=0'),
        @('Orders','Status',"In ('Pending','Confirmed','Packed','Out for Delivery','Delivered','Cancelled')"),
        @('Orders','PaymentMethod',"='Cash on Delivery'"),
        @('OrderItems','Quantity','>0'),
        @('OrderItems','Price','>=0'),
        @('OrderItems','Subtotal','>=0')
    )
    foreach ($rule in $rules) {
        $catalog.Tables.Item($rule[0]).Columns.Item($rule[1]).Properties.Item('Jet OLEDB:Column Validation Rule').Value = $rule[2]
    }
    # Optional text fields may legitimately contain empty strings from the editor.
    foreach ($pair in @(@('Categories','Description'),@('Products','Description'),@('Products','ImagePath'))) {
        $catalog.Tables.Item($pair[0]).Columns.Item($pair[1]).Properties.Item('Jet OLEDB:Allow Zero Length').Value = $true
    }
    $catalog.ActiveConnection.Close()
    Write-Host "Created $DatabasePath"
    Write-Host "Administrator: $AdminEmail (password not printed or stored in source)"
    Write-Host 'Next: open the Web Site in Visual Studio, enable HTTPS, match IIS/ACE bitness, and build.'
} catch {
    Write-Warning 'Initialization failed. A partially created database may exist. Do not use it; remove it manually and rerun after fixing the provider/schema error.'
    throw
} finally {
    if ($connection) { $connection.Dispose() }
    if ($catalog) { [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($catalog) }
    $hash = $null
}
