## Welcome to Self-Driving Car Simulator

This simulator was built to speedup and simplify AI dev part of our demo.
All the assets in this repository require Unity
Please follow the instructions below for the full setup.

### Unity Simulator User Instructions

1. Clone the repository to your local directory, please make sure to use
[Git LFS](https://git-lfs.github.com) to properly pull over large texture and model assets.

2. Install the free game making engine [Unity](https://unity3d.com), if you dont already have it. Unity is necessary to load all the assets.

3. Load Unity, Pick load exiting project and choice the `self-driving-car-sim` folder.

4. Load up scenes by going to Project tab in the bottom left, and navigating to
the folder Assets/1_SelfDrivingCar/Scenes. To load up one of the scenes,
for example the LakeTrack, double click the file LakeTrack.unity.
Once the scene is loaded up you can fly around it in the scene viewing window
by holding mouse right click to turn, and mouse scroll to zoom. To load demo setup
doulbe click Demo.unity file in the Scenes folder.

5. Play a scene. Jump into game mode anytime by simply clicking the top play
button arrow right above the viewing window.

6. View Scripts. Scripts are what make all the different mechanics of the
simulator work and they are located in two different directories,
the first is Assets/1_SelfDrivingCar/Scripts which mostly relate to the UI
and socket connections. The second directory for scripts is
Assets/Standard Assets/Vehicle/Car/Scripts and they control all the different
interactions with the car.

7. Building a scene to separate executable: Choose File->"Build Setting" menu
option. On the opened dialog choose the build type and the scenes to include.
Provide file name for the executable on prompt.

![Self-Driving Car Simulator](./sim_image.png)
