# PsyForge

A library for easy creation of 2D and 3D psychology experiments.

[Github Source Code](https://github.com/bruskatech/PsyForge)

## Overview

The experimental designs required to explore and understand neural mechanisms are becoming increasingly complex. There exists a multitude of experimental programming libraries for both 2D and 3D games; however, none provide a simple and effective way to handle high-frequency inputs in real-time (i.e., a closed-loop system). This is because the fastest these games can consistently react is once a frame (usually 30Hz or 60Hz). We introduce PsyForge, a framework for creating 2D and 3D experiments that handle high-frequency real-time data. It uses a safe threading paradigm to handle high precision inputs and outputs while still providing all of the power, community assets, and vast documentation of the Unity game engine. PsyForge supports most platforms such as Windows, Mac, Linux, iOS, Android, VR, and Web (with convenient psiTurk integration). Similar to other experimental programming libraries, it also includes common experimental components such as logging, configuration, text display, audio recording, language switching, and an EEG alignment system. PsyForge allows experimenters to quickly and easily create high quality, high throughput, cross-platform games that can handle high-frequency closed-loop systems.

For more information than what is in this document, please see the [Documentation Site](https://bruskatech.github.io/PsyForge).

## Setting Up a New Project

Here is how to make a new project.

1. Create a new unity project
1. Add PsyForge as a submodule to your project
    1. Open the Unity Package Manager (Top Menu Bar: Window > Package Manager)
    1. Click the plus in the top left corner and select "Install package from git URL"
    1. Paste the url for this github repo and click "Install"

    ```sh
    https://github.com/BruskaTech/PsyForge.git
    ```

    1. Wait for the install to finish (it may take a couple minutes)
1. Install the rounded UI package
    1. Click: Window > Package Manager
    1. Click: + (top left)
    1. Click: Install package from git URL
    1. Paste: `https://github.com/kirevdokimov/Unity-UI-Rounded-Corners.git#v3.5.0`
    1. Click: Install (on the right)
1. Copy everything in the Example folder into your main unity project at the top level.
    1. Go to the project window
    1. Click: Packages > PsyForge > Examples
    1. Copy all of the folders there into the *Assets* folder
1. Copy the *Installer/resources* and *Installer/configs* folders to your game directory.
    - When developing the experiment, it is always on your Desktop. 
    - When you build the experiment, it differs for each OS. In Linux and Macos, it is still on your Desktop. On Windows, it should be put inside the game folder on your desktop. 
    - (Recommended) You can also use a symlink for all of the different options so that you don't use extra data and it keeps it up to date in your git repo.

    ```bash
    # Symlink option (Linux/MacOS)
    ln -s $(realpath Installer/resources) ~/Desktop/.
    ln -s $(realpath Installer/configs) ~/Desktop/.

    # Symlink option (Windows)
    mklink /D <new_resource_path> <full_resource_path>
    mklink /D <new_config_path> <full_config_path>
    # Ex: mklink /D C:\Users\Name\Desktop\OpenField-1.0.1-Windows\resources C:\Users\Name\Documents\OpenField\Installer\resources

    # Copy option (Linux/MacOS)
    # You can also just use your file manager (Linux) or Finder (MacOS)
    cp Installer/resources ~/Desktop/.
    cp Installer/configs ~/Desktop/.

    # Copy option (Windows)
    # You can also just use a file explorer
    cp Install\resources\OpenField-<version>-Windows\resources C:\Users\<Name>\Desktop\OpenField-<version>-Windows\resources
    cp Install\resources\OpenField-<version>-Windows\configs C:\Users\<Name>\Desktop\OpenField-<version>-Windows\configs 
    ```

## Playing the Game

1. If you haven't already setup the resources and configs folder, do that first. That is explained in the section just above.
1. Open the manager scene (always play from this scene)
    1. Go to the project window
    1. Click: Assets > Scenes > PsyForgeExtensions
    1. Double click: manager
1. Click the play button at the top.

To just see the example, check the [Examples folder](https://github.com/BruskaTech/PsyForge/tree/main/Example).

## Types of Experimental Components Available

There are many types of experimental compoenents, but here are a few common ones. There is also a list of generally useful components.

### General Components

- Config
- Logging
- ErrorNotifier
- NetworkInterface
- InputManager
- List/Array shuffling (including ones that are consistent per participant)
- Random values that are consistent per participant

### Word List Experiments

- TextDisplay
- SoundRecorder
- VideoPlayer

### Spatial (3D) Experiments

- SpawnItems
- PickupItems

### Closed-Loop Experiments

- EventLoop
- ElememInterface

## FAQ

See the [FAQ](https://bruskatech.github.io/PsyForge/articles/FAQ.html).

## Authors

### PsyForge Authors

James Bruska

### UnityEPL 2.0 Authors

James Bruska, Connor Keane, Ryan Colyer

### UnityEPL 1.0 Authors

Henry Solberg, Jesse Pazdera
