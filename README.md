# 🍜 FoodExplorer — 美食探索与记录 App

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

### Phase 1 — Foundation & UI/UX Framework *(current)*
- [x] Shell-based navigation (AppShell)
- [x] MVVM architecture (ViewModels + Models)
- [x] Unified color theme with dark/light mode
- [x] WCAG accessibility support (SemanticProperties, font scaling)
- [x] Multi-page XAML structure (Home, RecipeList, RecipeDetail, Settings)
- [x] Base services and converters

### Phase 2 — Core Features & Hardware (1–2)
- [ ] Local JSON recipe data source
- [ ] Microphone / Voice search (Hardware #1)
- [ ] GPS Location & Food Map (Hardware #2)
- [ ] Search & filter functionality
- [ ] Favourites with local persistence
- [ ] Input validation and error handling

### Phase 3 — Advanced Hardware Features
- [ ] Shake-to-random-recipe (Accelerometer, Hardware #3)
- [ ] Text-to-Speech recipe narration (Hardware #4)
- [ ] Gyroscope / Magnetometer integration (Hardware #5)
- [ ] Haptic feedback & vibration (Hardware #6)

### Phase 4 — Deployment & Polish
- [ ] Android phone + Android tablet responsive layout
- [ ] Performance optimisation (image caching, lazy loading)
- [ ] Full code refactor (comments, classes, reusability)
- [ ] Final testing and bug fixes

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
- ⬜ Windows (Phase 4)

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

## ♿ Accessibility

This app follows **WCAG 2.1 AA** guidelines:
- All interactive elements have semantic labels (`SemanticProperties.Description`)
- Minimum touch target size: 44×44dp
- Colour contrast ratio ≥ 4.5:1 (normal text), ≥ 3:1 (large text)
- Dark mode support
- Scalable font sizes (Small / Medium / Large / Extra Large)
- Screen reader compatible

---

## 👤 Author

Student submission for Manchester Metropolitan University  
Department of Computing and Mathematics
