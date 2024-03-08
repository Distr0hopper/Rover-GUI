# Rover-GUI
Graphical User Interface for controlling a Mars Rover. Visualizing sensordata via Unity to create a pointcloud and controll the robot inside it.

![image](https://github.com/Distr0hopper/Rover-GUI/assets/100717485/3fd93800-ccdc-42e5-9c20-eaf282b74794)

# Installation 
The UI is developed under Unity Version 2022.3.10f1.
The project can be cloned and the folder "ROS_Mobile" needs to be opened in Unity. 
Because the [Unity-Robotics-Hub](https://github.com/Unity-Technologies/Unity-Robotics-Hub) package is used and it beeing in the Package Cache (which was modified for the implementation to work), two minor changes have to be done when opening the programm on a new machine.
**Otherwise errors will occure!**
* /ROS_Mobile/Library/PackageCache/com.unity.robotics.visualizations@c27f00c6cf/Runtime/Drawing3d/Scripts/PointCloudDrawing.cs
    * Line 12: Make the m_Mesh Mesh object public

* /ROS_Mobile/Library/PackageCache/com.unity.robotics.ros-tcp-connector@c27f00c6cf/Runtime/TcpConnector/ROSConnection.cs
    * Line 1016: Make the method InitializeHUD() public


# Functionalities 
* Change Camera FOV + Direction to look at
* Simple "Move 2 clicked point" interaction. Distance to point is shown, distance to arrival changes depending on how far the robot drove.
* Multiple Robots can be controlled by changing the dropdown at the top.
* Visualizing Connection-State and Watchdog for Camera and Sensory errors. Battery-Status and more will be implemented in the future.
* Manual Drive Mode to controll the Robot via Forward, Backward, Clockwise and Counterclockwise commands. All manual-steering commands in seconds (e.g. 5 Seconds Forward). Clockwise and counterclockwise given at angle.
* Mission-modes for making rock analysis (GeoSAMA), starting high-fidelity RIEGL scan and launching UWB beacons (UWB) over a Sensor Distribution System.

# Diagramm 
Methods in the Diagramm not complete, yet showing how the classes are connected
![Unity-Class-Diagramm](https://github.com/Distr0hopper/Rover-GUI/assets/100717485/6b3b3917-e0ee-4737-945d-11e3d806e5c6)
