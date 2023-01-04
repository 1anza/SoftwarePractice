Alivia Liljenquist & Lane Zaugg
December 9th, 2021

Server for TankWars game

When executed, server will run based on settings set in provided xml. Several game-mechanic parameters can be altered to suit the needs of 
the server operator. Upon successful initialization, server will state its functioning status, allowing it to receive clients. The server constructs
the world through the server's controller. This controllor creates an event loop that receives incoming clients and then listens to commands coming from each
connected client. These commands are collected and applied to the world frame-by-frame, sending updated information to each connected client on the current state of the world.
There are collision detection mechanics that determine if objects within the world "collide" with each other, causing specific events to occur depending on which objects collide.