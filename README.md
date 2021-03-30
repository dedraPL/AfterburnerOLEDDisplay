# INFO
Software implementation for OLED display of system info from MSI Afterburner.
Program read MSI Afterburner shared memory and send it via USB-Serial adapter to the hardware screen. 

# SOFTWARE

You need MSI Afterburner installed on your computer.

Hardware display project can be found on my other repo https://github.com/dedraPL/OLEDMonitor

<ins>You need both to work.</ins>

AfterburnerConnector C++ library reads data from Afterburner shared memory.

AfterburnerOledDisplay is a C# main project for this solution, reads system info via AfterburnerConnector library and sends it via Serial to the hardware screen.

You can change COM port, monitored GPU, refresh rate or disable notifications with this application.

# PACKET
Serial output send MessagePack format packets.

Key | Value | Description
--- | ----- | -----------
GT | uint8 | GPU Temperature
GU | uint8 | GPU Usage (buffored on graph)
CT | uint8 | CPU Temperature
CU | uint8 | CPU Usage (buffored on graph)
FR | uint8 | Framerate (buffored on graph)

# TODO

- [x] working basic version
- [x] GUI with tray icon 
- [ ] RAM usage reader
- [ ] screens configuration for hardware display
- [ ] graphs in the GUI window