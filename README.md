[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/uM_GSLJS)

# 🍜 FoodExplorer — Food Discovery & Recording App

**Module:** 6G6Z0014 – Mobile Computing  
**Assessment:** 1CWK100 – Developing a Cross-Platform Mobile App  
**Framework:** .NET MAUI (Multi-platform App UI)  
**Theme:** Food & Drink

---

## 📱 App Overview

FoodExplorer is a cross-platform mobile application that allows users to discover, record, and share their food experiences. The app combines rich recipe browsing with smart hardware integrations to deliver an immersive culinary journey.

### Core Features

| Feature | Description |
|---|---|
| 🏠 Home | Curated featured recipes and quick-access categories |
| 📋 Recipe List | Browse, search and filter a full recipe library |
| 🔍 Recipe Detail | Step-by-step instructions with nutritional info |
| ⚙️ Settings | Theme toggle, font size, accessibility preferences |

---

## 🛠️ Development Plan

### Phase 1 — Foundation & UI/UX Framework *(complete)*

- ✅ Shell-based navigation (AppShell)
- ✅ MVVM architecture (ViewModels + Models)
- ✅ Unified color theme with dark/light mode
- ✅ WCAG accessibility support (SemanticProperties, font scaling)
- ✅ Multi-page XAML structure (Home, RecipeList, RecipeDetail, Settings)
- ✅ Base services and converters

### Phase 2 — Core Features & Hardware (1–2) *(complete)*

- ✅ Local JSON recipe data source
- ✅ Camera / dish photo capture (Hardware #1)
- ✅ Microphone / Voice search (Hardware #2)
- ✅ Search & filter functionality
- ✅ Favourites with local persistence
- ✅ Input validation and error handling

### Phase 3 — Advanced Hardware Features *(complete)*

- ✅ Shake-to-random-recipe (Accelerometer, Hardware #3)
- ✅ Text-to-Speech recipe narration (Hardware #4)
- ✅ Gyroscope / Magnetometer integration (Hardware #5 & #6)
- ✅ Haptic feedback & vibration (Hardware #7 & #8)
- ✅ Geolocation — find nearby restaurants (Hardware #9)

### Phase 4 — Deployment & Polish *(complete)*

- ✅ Android phone + Android tablet responsive layout
- ✅ Performance optimisation (image caching, lazy loading)
- ✅ Full code refactor (comments, classes, reusability)
- ✅ Final testing and bug fixes

---

## 🚀 Getting Started

### Prerequisites
- Visual Studio 2022 (v17.8+) with .NET MAUI workload
- .NET 8 SDK
- Android Emulator (API 33+) or physical Android device

### Running the App

1. Open `FoodExplorer.sln` in Visual Studio 2022 (or run from the `FoodExplorer` folder)

2. Select your target platform (Android recommended)

3. Press **F5** or click **Run**

```bash
cd FoodExplorer
dotnet build -f net8.0-android
```

### Deployment Targets
- ✅ Android Phone Emulator (primary)
- ✅ Android Tablet Emulator
- ✅ Windows (WinUI 3)

---

## 📁 Project Structure

```
FoodExplorer/
├── Models/            # Data models (Recipe, Category, etc.)
├── ViewModels/        # MVVM ViewModels
├── Views/             # XAML pages
├── Services/          # Business logic & hardware services
├── Converters/        # XAML value converters
├── Controls/          # Reusable custom controls
└── Resources/
    ├── Styles/        # Global colours and styles
    ├── Fonts/         # Custom font files
    └── Images/        # App icons and images
```

---

## 🎯 Hardware Features

| # | Feature | API Used | Where Used |
|---|---------|----------|------------|
| 1 | Camera | `MediaPicker.CapturePhotoAsync` | Recipe Detail — Capture dish photo |
| 2 | Microphone | `SpeechRecognizer` (Android) | Recipe List — Voice search |
| 3 | Accelerometer | `Accelerometer.Default` | Home — Shake for random recipe |
| 4 | Text-to-Speech | `TextToSpeech.Default.SpeakAsync` | Recipe Detail — Read recipe aloud |
| 5 | Gyroscope | `Gyroscope.Default` | Recipe Detail — Tilt to change step |
| 6 | Compass | `Compass.Default` | Recipe Detail — Show heading |
| 7 | Haptic Feedback | `HapticFeedback.Default.Perform` | Throughout — Button feedback |
| 8 | Vibration | `Vibration.Default.Vibrate` | Error states & shake confirm |
| 9 | Geolocation | `Geolocation.Default.GetLocationAsync` | Recipe Detail — Find nearby restaurants |

---

## ♿ WCAG 2.1 Compliance

| Criterion | Implementation |
|-----------|---------------|
| 1.3.1 Info and Relationships | `SemanticProperties.HeadingLevel` on all section headers |
| 1.4.3 Contrast (Minimum) | Colour contrast ≥ 4.5:1 for normal text |
| 1.4.4 Resize Text | 4-level font scaling via `AccessibilityService` |
| 1.4.10 Reflow | Single-column layout with `ScrollView` throughout |
| 2.4.2 Page Titled | All pages have `Title` bound to ViewModel |
| 2.5.5 Target Size | All interactive elements ≥ 44×44dp (`MinimumHeightRequest="44"`) |
| 4.1.2 Name, Role, Value | `SemanticProperties.Description` on all controls |

---

## 🚀 Deployment

| Target | Status | Notes |
|--------|--------|-------|
| Android Phone Emulator | ✅ | Primary development target (API 33+) |
| Android Tablet Emulator | ✅ | Responsive layout via `DeviceLayoutService` (2/3/4 column grid) |
| Windows (WinUI 3) | ✅ | Visual Studio 2022 — `net8.0-windows10.0.19041.0` |

---

## ♿ Accessibility (Summary)

This app follows **WCAG 2.1 AA** guidelines:
- All interactive elements have semantic labels (`SemanticProperties.Description`)
- Minimum touch target size: 44×44dp
- Colour contrast ratio ≥ 4.5:1 (normal text), ≥ 3:1 (large text)
- Dark mode support
- Scalable font sizes (Small / Medium / Large / Extra Large)
- Screen reader compatible (`SemanticScreenReader.Announce`)

---

## 👤 Author
ZixuanLiu  
Student submission for Manchester Metropolitan University
