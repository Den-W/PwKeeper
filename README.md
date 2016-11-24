*****************   PasswordKeeper v1.04   ***********************

This is a simple hand made device for storing your passwords.
Essentially, it emulates USB keyboard and should work with any device where USB keyboard may be connected.
It sends user-defined sequences of keypresses as if they were entered from keyboard.
<<<<<<< HEAD
To simplify password management in token Windows program PwKeeperPC.exe is included into the package.
Also, serial console added to device. It allows config device from *nix systems.

------------------------------------------------------------------
How to write HEX to board:

1) Download and install software from arduino.cc
2) Connect your board to PC and install all necessary drivers.
3) Open DeviceManager window. Open COM and LPT node. There you should see you board as virtual COM port.
4) On Arduino board connect RST pin to GND.
5) In COM and LPT node new COM port should appear on ~8 sec. And then it replaced by port, which you have seen already.
   If you do not see new ports to appear try to repeat step 4, but connect RST to GND 2 times with interval of 1 sec between tries.
6) Replace COM16 in command in CMD file with port, which appears temporarily.
   Check pathes to AVRDUDE.EXE and avrdude.conf in command.
   Correct it according to your system.
7) Connect RST pin to GND. Start CMD and remove RST to GND connection.
   Download to board should pass.

Essentialy, COM port that you see when board is in idle state - it is HID port. 
It is not the LOADER port. Board LOADER COM appears only on short period after reset.
-------------------------------------------------------------------   

Changes history:

v1.4 - 24.11.2016
	- *nix-like TTY console added to PwKeeper
	- Commands description added to rtf.
=======
To simplify password management in token Windows program PwKeeperPC.exe is included into package.
>>>>>>> origin/master

v1.3 - 16.11.2016
	- Several bugfixes in PwKeeperPC.exe
	- Several bugfixes in PwKeeper.hex
 	- *.stl files added
	- Quick access buttons added to wiring diagram

v1.2 - 10.11.2016 
	- First public release
