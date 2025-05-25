# CounterSplit

CounterSplit is a Windows Forms application designed to track in-game statistics for **God of War** speedruns. It provides real-time tracking of key metrics such as in-game time (IGT), hit counts, health points, and more. The application is tailored for speedrunners aiming for precision and performance in their runs.

---

## Features

- **Real-Time Tracking**: Displays in-game time (IGT), last recorded time (LRT), current health, total damage, and hit counts.
- **Segment Splits**: Automatically tracks and displays splits for different game segments.
- **Memory Integration**: Reads game data directly from PCSX2 emulator's memory for accurate tracking.
- **Customizable UI**: Minimalistic and user-friendly interface with drag-and-drop support.
- **Keyboard Shortcuts**: Includes hotkeys for debugging and timer control.
- **Export Splits**: Save your splits to a file for later analysis.

---

## Requirements

- **.NET 8.0 SDK** or later.
- **PCSX2 Emulator** (Qt version).
- **MouseKeyHook Library** (included as a NuGet dependency).

---

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/your-username/CounterSplit.git
    cd CounterSplit
    ```

2. Build the project:
    ```bash
    dotnet build
    ```

3. Run the application:
    ```bash
    dotnet run --project GowDamagelessSTD.csproj
    ```

---

## Usage

1. Launch the PCSX2 emulator and start **God of War**.
2. Open CounterSplit.
3. The application will automatically detect the emulator process and begin tracking.
4. Use the following keyboard shortcuts:
    - `F7`: Debug mode.
    - Right-click anywhere on the app to access the context menu for additional options.

---

## Development

### Project Structure

- **`MainForm.cs`**: Core logic for the application's UI and memory reading.
- **`GowDamagelessSTD.csproj`**: Project configuration and dependencies.
- **`Program.cs`**: Entry point for the application.
- **`MainForm.Designer.cs`**: Auto-generated UI layout code.

### Dependencies

- [MouseKeyHook](https://www.nuget.org/packages/MouseKeyHook): For global keyboard and mouse event handling.

---

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository.
2. Create a new branch:
    ```bash
    git checkout -b feature-name
    ```
3. Commit your changes:
    ```bash
    git commit -m "Add feature-name"
    ```
4. Push to your branch:
    ```bash
    git push origin feature-name
    ```
5. Open a pull request.

---

## License

This project is licensed under the [MIT License](LICENSE).

---

## Acknowledgments

- Inspired by the speedrunning community.
- Special thanks to the developers of PCSX2 and the MouseKeyHook library.

---

Enjoy your speedruns with CounterSplit! 🎮  