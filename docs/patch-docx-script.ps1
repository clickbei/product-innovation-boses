# patch-docx-script.ps1
# Rewrites the file-writing block in Generate-UserGuideDocx.ps1

$target = "C:\Users\AJumagdoa\source\repos\product-innovation-boses\docs\Generate-UserGuideDocx.ps1"
$lines  = [IO.File]::ReadAllLines($target)

# Locate anchor line
$startIdx = -1
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match '#.*\[Content_Types\]|# Write package files') { $startIdx = $i; break }
}
if ($startIdx -lt 0) { Write-Error "Anchor not found"; exit 1 }

# Locate end (line containing numbering.xml WriteAllText)
$endIdx = -1
for ($i = $startIdx; $i -lt $lines.Count; $i++) {
    if ($lines[$i] -match 'WriteAllText.*numbering') { $endIdx = $i; break }
}
if ($endIdx -lt 0) { Write-Error "End not found"; exit 1 }

Write-Host "Replacing lines $startIdx to $endIdx"

# Build the replacement as an array of plain strings (no nested here-strings)
$r = [Collections.Generic.List[string]]::new()
$r.Add('# Write package files using IO.Path.Combine to avoid PS5.1 bracket-globbing')
$r.Add('$ctFile      = [IO.Path]::Combine($tmpDir, "[Content_Types].xml")')
$r.Add('$relsFile    = [IO.Path]::Combine($tmpDir, "_rels", ".rels")')
$r.Add('$docRelsFile = [IO.Path]::Combine($tmpDir, "word", "_rels", "document.xml.rels")')
$r.Add('$docFile     = [IO.Path]::Combine($tmpDir, "word", "document.xml")')
$r.Add('$stylesFile  = [IO.Path]::Combine($tmpDir, "word", "styles.xml")')
$r.Add('$numFile     = [IO.Path]::Combine($tmpDir, "word", "numbering.xml")')
$r.Add('')
$r.Add('$ctXml = "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?>" + [char]10 +')
$r.Add('  "<Types xmlns=`"http://schemas.openxmlformats.org/package/2006/content-types`">" + [char]10 +')
$r.Add('  "  <Default Extension=`"rels`" ContentType=`"application/vnd.openxmlformats-package.relationships+xml`"/>" + [char]10 +')
$r.Add('  "  <Default Extension=`"xml`"  ContentType=`"application/xml`"/>" + [char]10 +')
$r.Add('  "  <Override PartName=`"/word/document.xml`"  ContentType=`"application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml`"/>" + [char]10 +')
$r.Add('  "  <Override PartName=`"/word/styles.xml`"    ContentType=`"application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml`"/>" + [char]10 +')
$r.Add('  "  <Override PartName=`"/word/numbering.xml`" ContentType=`"application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml`"/>" + [char]10 +')
$r.Add('  "</Types>"')
$r.Add('')
$r.Add('$relsXml = "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?>" + [char]10 +')
$r.Add('  "<Relationships xmlns=`"http://schemas.openxmlformats.org/package/2006/relationships`">" + [char]10 +')
$r.Add('  "  <Relationship Id=`"rId1`" Type=`"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument`" Target=`"word/document.xml`"/>" + [char]10 +')
$r.Add('  "</Relationships>"')
$r.Add('')
$r.Add('$docRelsXml = "<?xml version=`"1.0`" encoding=`"UTF-8`" standalone=`"yes`"?>" + [char]10 +')
$r.Add('  "<Relationships xmlns=`"http://schemas.openxmlformats.org/package/2006/relationships`">" + [char]10 +')
$r.Add('  "  <Relationship Id=`"rId1`" Type=`"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles`"   Target=`"styles.xml`"/>" + [char]10 +')
$r.Add('  "  <Relationship Id=`"rId2`" Type=`"http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering`" Target=`"numbering.xml`"/>" + [char]10 +')
$r.Add('  "</Relationships>"')
$r.Add('')
$r.Add('[IO.File]::WriteAllText($ctFile,      $ctXml)')
$r.Add('[IO.File]::WriteAllText($relsFile,    $relsXml)')
$r.Add('[IO.File]::WriteAllText($docRelsFile, $docRelsXml)')
$r.Add('[IO.File]::WriteAllText($docFile,     $docXml)')
$r.Add('[IO.File]::WriteAllText($stylesFile,  $stylesXml)')
$r.Add('[IO.File]::WriteAllText($numFile,     $numberingXml)')

$newLines = [Collections.Generic.List[string]]::new()
foreach ($l in $lines[0..($startIdx-1)])          { $newLines.Add($l) }
foreach ($l in $r)                                 { $newLines.Add($l) }
foreach ($l in $lines[($endIdx+1)..($lines.Count-1)]) { $newLines.Add($l) }

[IO.File]::WriteAllLines($target, $newLines)
Write-Host "Done. Lines: $($newLines.Count)"
