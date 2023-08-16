Hi 

First! Thanks to bought the kit :) Really!

I hope it will give you satisfaction. But please, no matter the opinion you have 
(i prefried good, but i play the game), please take 5 minutes of your time to leave a 
review and put the stars that seem right to you... That really help us.

But pity not to use the review as weir anger or to request support. For that use : 
    ----
    
black.creepy.cat@gmail.com

Why ? Just because i check my mail more than the unity store reviews... 

-------------
USING THE KIT
-------------

- Create a new project
- Into BUILD SETTING / OTHER SETTINGS put the rendering mode into "deferred / linear" instead "forward / gamma"
- switch the camera to "Legacy Deferred" or "Deferred" 		
- Import the package
- read the FAQ about doors or any "static" objects, and compute lightmap.


-------------------------------------------------------------------------------------
Here are a series of questions and answers about the kit (I love talking to myself) :
-------------------------------------------------------------------------------------

---------------------------------------------------------------------
"Hell! I compute lightmap but my rendering is black or/and glitched?"
---------------------------------------------------------------------

- Don't panic, it's a unity problem with lightmap, to fix it : Clear the GI cache first, and next delete the 
  directory called "Example_01" and recompute lightmap.


---------------------------------------
"Those damned doors did not open? Why?"
---------------------------------------

- Keep calm :) Do not write a crappy review now :) The problem due to the static mode! Unity do not want move 
  objects flagged as static… You just have to search the gameobjects names "Door_Left" and "Door_Right", select
  them all and flag them static before lightmap

  I can not tell you the number of times, after two hours of lightmap computing, i realized that the doors were 
  in static = off...)
  
  
--------------------------------------------------------------
"Creepy Cat! you suxx so much! what this bling-bling world???"
--------------------------------------------------------------
- If you find the level to much metalic, you can easily attenuate the effect! Just play
  with photoshop (or whatever you want),  with the brightness/contrast of the metalic maps (_Metal.tiff)
  to increase/remove the effect (try to play with the alpha channel too...  :)
  
I wanted to make a level more bling bling and clean. I do not like the dark and murky scifi envs :) 


--------------------------------------------------------------
"It's cool! But i want to do my hown creepy/darky textures?"
--------------------------------------------------------------
On this issue, I would not say that this will be super easy and fast, only one person who 
knows more or less photoshop can pass the race about the re-creation of textures, to make a good
adaptation for your project.

But I sincerely know that this is possible! and to help you in this direction :) I provide the 
flat textures (_Flat.png into the zip file), and if you use them in photoshop + the normal maps (_Norm.png)
it will be possible easily recreate all the layers and then apply the effect/textures you want.  choice.

It's a bit of work, yes! But it's possible. The kit has been made to this direction.

Note : Do not panic, i do not plan to move/modify/scale the position of the elements 
under the textures (i say that if you want to batch your hown), but i think add some 
new texture in the futur.

----------------------------------------------------------------------------------------------
"Creepy Cat! you suxx so much again! why do not provide texture in photoshop files? directly?"
----------------------------------------------------------------------------------------------
It's just because i don't do them under photoshop! but directly under my 3D software... To get good
diffuse/normal/ao/metal maps, i have my hown generation method, based on my experience. 

My personnal cooking, if you prefer :) That's why I did not have files with layers. 

But a motivated team that will use my kit as a graphic base, will be able to easily re-custom all the kit. 


--------------------
"Any update planed?"
--------------------
Maybe, i plan a quick short time update, to put the rest of the objects i want to give (but my mind for the moment
is saturate, relase needed... it's always like this with big projects), And later, a more consistant
updates based on the users demands... 

I would take the ideas that I think are the best for the whole pack.


-------------------------------------------------------------------------------------
"Oh my god! there is a missing object! (There is always a missing object in a kit...)
-------------------------------------------------------------------------------------
To adapt a missing object to your kit, feel free to serve you some parts of textures!

Indeed, the kit is made with the rules of the modular design, and the textures also go
to this direction, it is possible, for example, with the texture: 

Wall_Atlas_05_Dif.png 

To create (for those who know a 3D software) a calculation server ? or whatever
you want/need? just stay within each of textures and make your object based on them.

Wall_Atlas_01_Dif.png

By example can be used to create other specials missing walls for your project! 

Because yes!, create a video game, takes a bit of work .... 

To believe a kit solves all your game design issues for $30, and without working, is a heresy :)

A kit is a starting base! I will try one day to make a video to explain an example of 
re-using of the textures to create a needed/missing object.

The prefab "Station_Radar_01" is a pure example of my previous explains, easily made
with the kit textures, same for the prefab "Station_Base_01" etc...

And one time again :) Thanks to bought the kit :)


-----------
"KNOW BUGS" 
-----------

- I notify a random coloring bug on mac, not pc, if the scene coloring is "red" switch the camera 
  to "Legacy Deferred" 																						
																						
																						
																						
													Creepy Cat
























																																																																																																									