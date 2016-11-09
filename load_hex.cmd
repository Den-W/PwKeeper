:: 1) Check pathes to AVRDUDE.EXE and avrdude.conf in command below. Correct it according to your system.
:: 2) Open DeviceManager window. Open COM and LPT node.
:: 3) On Arduino Leonardo connect RST pin to GND 2 times with interval of 1 sec between tries.
:: 4) In COM and LPT node new COM port should appear on ~8 sec. Replace COM16 in command below with your port.
:: 5) Connect RST pin to GND 2 times with interval of 1 sec between tries and immediately run LOAD_HEX.CMD.
:: 6) Download to board should pass.

"C:\Program Files (x86)\Arduino\hardware\tools\avr/bin/"avrdude -C"C:\Program Files (x86)\Arduino\hardware\tools\avr/etc/"avrdude.conf -v -patmega32u4 -cavr109 -PCOM16 -b57600 -D -Uflash:w:PwKeeper.ino.hex:i 
