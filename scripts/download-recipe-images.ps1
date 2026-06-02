# Downloads verified food photos for recipe_*.jpg (TheMealDB / Unsplash / Wikimedia).
$dir = Join-Path $PSScriptRoot "..\FoodExplorer\Resources\Images"
$q = "?w=600&h=400&fit=crop&q=80"
$wiki = { param($file) "https://commons.wikimedia.org/w/index.php?title=Special:Redirect/file/$file&width=600" }

# Each entry: file name + ordered URL fallbacks (first success wins).
# URLs were verified to match the dish name before inclusion.
$items = @(
  @{ file = "recipe_pizza.jpg";        urls = @("https://images.unsplash.com/photo-1574071318508-1cdbab80d002$q") },
  @{ file = "recipe_ramen.jpg";        urls = @("https://images.unsplash.com/photo-1569718212165-3a8278d5f624$q") },
  @{ file = "recipe_tacos.jpg";        urls = @("https://images.unsplash.com/photo-1565299585323-38d6b0865b47$q") },
  @{ file = "recipe_cheesecake.jpg";   urls = @("https://images.unsplash.com/photo-1533134242443-d4fd215305ad$q") },
  @{ file = "recipe_carbonara.jpg";    urls = @("https://www.themealdb.com/images/media/meals/llcbn01574260722.jpg") },
  @{ file = "recipe_padthai.jpg";      urls = @(
      (& $wiki "Pad_Thai.JPG"),
      "https://upload.wikimedia.org/wikipedia/commons/thumb/3/39/Pad_Thai.JPG/600px-Pad_Thai.JPG"
    ) },
  @{ file = "recipe_guacamole.jpg";    urls = @("https://images.unsplash.com/photo-1700625916627-16ad4fb0553c$q") },
  @{ file = "recipe_butter_chicken.jpg"; urls = @("https://images.unsplash.com/photo-1603894584373-5ac82b2ae398$q") },
  @{ file = "recipe_burger.jpg";        urls = @("https://images.unsplash.com/photo-1568901346375-23c9450c58cd$q") },
  @{ file = "recipe_pancakes.jpg";     urls = @("https://images.pexels.com/photos/376464/pexels-photo-376464.jpeg?auto=compress&cs=tinysrgb&w=600&h=400") },
  @{ file = "recipe_bibimbap.jpg";     urls = @("https://images.unsplash.com/photo-1498654896293-37aacf113fd9$q") },
  @{ file = "recipe_fried_rice.jpg";   urls = @(
      "https://foodish-api.com/images/rice/rice1.jpg",
      (& $wiki "Fried_rice.jpg")
    ) },
  @{ file = "recipe_paella.jpg";       urls = @(
      "https://www.themealdb.com/images/media/meals/5r5rvx1763287943.jpg",
      "https://www.themealdb.com/images/media/meals/c6ghxm1763335584.jpg",
      "https://images.unsplash.com/photo-1534080564583-6be75777b70a$q"
    ) },
  @{ file = "recipe_poke.jpg";         urls = @(
      "https://images.unsplash.com/photo-1768326119213-e0ad875083a3$q",
      "https://images.unsplash.com/photo-1670816978291-a5cf23d87968$q"
    ) },
  @{ file = "recipe_brownies.jpg";     urls = @("https://images.unsplash.com/photo-1606313564200-e75d5e30476c$q") },
  @{ file = "recipe_kungpao.jpg";      urls = @("https://www.themealdb.com/images/media/meals/1525872624.jpg") },
  @{ file = "recipe_sweet_sour_pork.jpg"; urls = @("https://www.themealdb.com/images/media/meals/1529442316.jpg") },
  @{ file = "recipe_mapo_tofu.jpg";    urls = @("https://www.themealdb.com/images/media/meals/1525874812.jpg") }
)

$failed = @()
foreach ($item in $items) {
  $out = Join-Path $dir $item.file
  $ok = $false
  foreach ($url in $item.urls) {
    try {
      $code = curl.exe -s -L -m 20 -o $out -w "%{http_code}" $url
      if ($code -ne "200") { throw "HTTP $code" }
      $len = (Get-Item $out).Length
      if ($len -lt 12000) { throw "File too small ($len bytes)" }
      Write-Host "OK $($item.file) ($len bytes)"
      $ok = $true
      break
    }
    catch {
      Write-Warning "  try failed for $($item.file): $url ($($_.Exception.Message))"
    }
  }
  if (-not $ok) {
    if ((Test-Path $out) -and ((Get-Item $out).Length -ge 12000)) {
      Write-Host "KEEP $($item.file) (existing $((Get-Item $out).Length) bytes)"
      $ok = $true
    } else {
      $failed += $item.file
    }
  }
}

if ($failed.Count) {
  Write-Error "Failed: $($failed -join ', ')"
  exit 1
}

$groups = Get-ChildItem (Join-Path $dir "recipe_*.jpg") | ForEach-Object {
  [PSCustomObject]@{ Name = $_.Name; Hash = (Get-FileHash $_.FullName -Algorithm MD5).Hash }
} | Group-Object Hash | Where-Object { $_.Count -gt 1 }
if ($groups) {
  Write-Warning "Duplicate image files detected:"
  $groups | ForEach-Object { Write-Warning "  $($_.Name): $($_.Group.Name -join ', ')" }
}

Write-Host "Done. $($items.Count) recipe images."
