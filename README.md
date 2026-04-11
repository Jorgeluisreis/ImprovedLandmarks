<div align="center">
  <img src="https://raw.githubusercontent.com/Jorgeluisreis/ImprovedLandmarks/refs/heads/main/Images/Icon.png" alt="Improved Landmarks" width="150"/>
  <br><br>
  <a href="https://github.com/Jorgeluisreis/ImprovedLandmarks/releases">
    <img src="https://img.shields.io/github/v/release/Jorgeluisreis/ImprovedLandmarks?logo=github" alt="Release">
  </a>
  <a href="https://github.com/Jorgeluisreis/ImprovedLandmarks/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/Jorgeluisreis/ImprovedLandmarks" alt="License">
  </a>
  <img src="https://img.shields.io/badge/game%20version-1.5.9.8%2B-blue" alt="Game Version">
  <img src="https://img.shields.io/badge/dependency-UITools%201.1.5-orange" alt="UITools">
</div>

---

# Improved Landmarks

A mod for **Spaceflight Simulator** that allows you to place fully customizable landmarks anywhere on the map.

---

## Features

- **Place landmarks anywhere**: click anywhere on the map to drop a landmark on the closest planet
- **Custom name**: name each landmark freely
- **Label direction**: choose which direction the label appears relative to the pin (8 directions, 15° steps)
- **Pin size**: adjust the pin dot size
- **Label size**: adjust the font size of the label text
- **Label spacing**: control the distance between the pin and the label
- **Position fine-tuning**: move an existing landmark precisely using directional buttons with adjustable step size
- **Edit existing landmarks**: rename and adjust all properties of any placed landmark
- **Delete landmarks**: remove any landmark with a confirmation prompt
- **Persistent storage**: landmarks are saved and restored on every session
- **Draggable window**: the landmark panel is draggable and remembers its position across sessions
- **Per-object name labels**: toggle the display of a rocket's name directly on the map for any rocket (Show/Hide Name button)
- **Automatic label cleanup**: when a rocket is destroyed or removed, its label is automatically removed from the save file
- **Auto-update**: automatically updated via UITools when a new release is available

---

## Requirements

| Dependency | Minimum Version |
|------------|----------------|
| Spaceflight Simulator | 1.5.9.8 |
| [UITools](https://github.com/cucumber-sp/UITools) | 1.1.5 |

---

## Installation

1. Make sure **UITools** is installed
2. Download the latest `ImprovedLandmarks.dll` from [Releases](https://github.com/Jorgeluisreis/ImprovedLandmarks/releases)
3. Place the `.dll` inside your mods folder (in-game: Mod Loader > Open Mods Folder):
   ```
   Spaceflight Simulator/Mods/ImprovedLandmarks/ImprovedLandmarks.dll
   ```
4. Launch the game

---

## Usage

### Landmarks

1. Load into a world and open the **map view**
2. Click **"+ Landmark"** to enter placement mode
  <img width="362" height="284" alt="create_landmark" src="https://github.com/user-attachments/assets/4d664ec2-5ef1-4bbd-b599-d45333c2a7d6" />
  
3. Click anywhere on the map, a dialog will open
  <img width="752" height="580" alt="data_landmark" src="https://github.com/user-attachments/assets/fc59fb7f-fd50-4b89-9e46-f6faa714ad7f" />
  
4. Set the landmark name and customize appearance
  <img width="722" height="482" alt="data_landmark2" src="https://github.com/user-attachments/assets/b94db34e-74bc-479e-a7a6-601d6ff805a8" />
  
5. Click **Confirm** to save

To edit or delete a landmark, expand the list and use the **Edit** / **Del** buttons next to each entry.

<img width="600" height="378" alt="show_list_landmark" src="https://github.com/user-attachments/assets/c1b599d6-b12c-424e-9964-c8f1126578eb" />
<img width="684" height="566" alt="edit_landmark" src="https://github.com/user-attachments/assets/b4b11a94-2cbb-42d4-869d-cfc5e96fde15" />

---
Great, now all you have to do is enjoy creating your landmarks.

<img width="454" height="320" alt="result_landmark" src="https://github.com/user-attachments/assets/1c9e9b98-b342-4568-9c0d-7097ed475c14" />

---

### Object Name Labels

You can now toggle the display of any rocket's name directly on the map! This feature is persistent and automatically cleans up labels when rockets are destroyed or removed.

1. **Select a object** on the map (click its icon).
<img width="329" height="301" alt="image" src="https://github.com/user-attachments/assets/b7051fb2-4b8b-4949-a181-307b41e2ad02" />

2. Next to the Rename button, click **Show/Hide Name**.
<img width="388" height="362" alt="image" src="https://github.com/user-attachments/assets/a3dbbbf2-608e-4b03-9246-749c2de850d7" />

  - If the name is hidden, it will appear above the rocket on the map.
  - If the name is shown, clicking again will hide it.
  
  <img width="312" height="302" alt="image" src="https://github.com/user-attachments/assets/f1fa22c8-3cb3-4d57-858f-fb770962b110" />

  <img width="893" height="724" alt="image" src="https://github.com/user-attachments/assets/483e6dd8-6ef9-4045-8411-507d0f3ea56b" />
---
