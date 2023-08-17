-----------------
Original readme :
-----------------
Hi dear customer (or CG Persia member) 

Pfiuuuu... I'm tired... Limit burnout, but i'm happy to release this package! :)

Indeed, it is (I think) my kit the most graphically finished... I redid this kit three times during the period of 2017/2018... And every time I was not satisfied by the look, or the way of building things.

Because you must know that it is very complicated to make a kit, if you make a lot of parts, the kit becomes too complicated to use for the customer's, and if you make big block easy to assemble, the kit becomes too simple .. And the customers are not happy because lacks of liberty:)

But i hope you will like to use it! I try always to put love into my products... I just hope you feel it, and one time again :) Thanks to bought the kit :)

And about people illegally downloaded it? : Me too, i downloaded bunch of stuff when i was younger, I will not play the moralizers, but if you intend to use it in a paid production, buy at least a license ... The only thing i ask is : Do no sell my datas, including other art stores like ‘Second Life’, ‘Unreal UDK store’, ‘Turbo Squid’ etc… 

In any case, take pleasure with my work and please, write a review :)


									         Creepy Cat

             _,'|             _.-''``-...___..--';)
           /_ \'.      __..-' ,      ,--...--''''
          <\    .`--'''       `     /'
           `-';'               ;   ; ;
     __...--''     ___...--_..'  .;.'
    (,__....----'''       (,..--''   





---------------
USING THE KIT :
---------------

- Create a new project
- Into BUILD SETTING / PLAYER SETTINGS put the rendering mode into "deferred / linear" instead "forward / gamma"
- switch the camera to "Legacy Deferred" or "Deferred" 	(The project as been made for deferred)	
- Import the package
- read the FAQ about doors or any "static" objects, and compute lightmap.
- Via the package manager, install the "Post Processing Effects"
- Once made, you can use the file : Post-Processing Profile on your camera to get the same result than the video.

Links about post processing :
-----------------------------
https://docs.unity3d.com/Packages/com.unity.postprocessing@2.0/manual/index.html
 

-----------
About PBR :
-----------

By default, I put a clean (without so much scratchs) detailled map : Detail_01.png with a tilling of two, but try the 
others detailed maps i provide : Detail_02.png (for startrek clean)/Detail_03.png (for aged rendering), try to use 
them to make tests :)

I also provide heightmap textures (do not confuse with the standard shader height map params), with my heightmap
you can make your texture mixture with photoshop, use => id map + ao + height to create some others diffuses.

--------------------------------------------------------------------------------------------------------------------

But PITY!!! Do not use the reviews to ask for help or to get rid of your anger. For that use : 
    
black.creepy.cat@gmail.com

If you want to see my youtube channel some interesting stuff on : 
https://www.youtube.com/channel/UCvNtMt39uh_nJFZGT6qjgAQ/videos?flow=list&view=57

Check my Facebook page for fresh informations : https://www.facebook.com/BlackCreepyCat/

--------------------------------------------------------------------------------------------------------------------





----------------------------------------------------------
Here are a series of questions and answers about the kit :
----------------------------------------------------------

---------------------------------------------------------------------
"Hell! I compute lightmap but my rendering is black or/and glitched?"
---------------------------------------------------------------------

- Don't panic, it's a unity problem with lightmap, to fix it : Clear the GI cache first, and next delete the 
  directory called "Example_Map_01" and recompute the lightmap.


---------------------------------------
"Those damned doors did not open? Why?"
---------------------------------------

- Keep calm :) Do not write a crappy review now :) The problem due to the static mode! Unity do not want move 
  objects flagged as static… You just have to search the gameobjects names "Door_L" and "Door_R", select
  them all and flag them static before lightmap

  I can not tell you the number of times, after few hours of lightmap computing, i realized that the doors were 
  in static = off...)
  

--------------------------------------------------------------
"It's cool! But i want to do my hown creepy/darky textures?"
--------------------------------------------------------------
It's a bit of work, yes! But it's possible. The kit has been made to this direction.

You can use a programm that accept the textures ID, like DDO (Quixel) or if i remember Substance Painter.
The kit has been made to help you to use the albedo textures into those softwares.

----------------------------------------------------------------------------------------------
"Creepy Cat! you suxx so much again! why do not provide texture in photoshop files? directly?"
----------------------------------------------------------------------------------------------
It's just because i don't do them under photoshop! but directly under my 3D software... To get good
diffuse/normal/ao/metal maps, i have my hown generation method, based on my experience. 

My personnal cooking, if you prefer :) That's why I did not have files with layers. 

But a motivated team that will use my kit as a graphic base, will be able to easily re-custom all the kit. 


---------------------
"Any updates planed?"
---------------------
My mind is saturated for the moment, but i plan three updates : 

- The first one (soon as possible) : The rest of the objects i want to add, some office furnitures
or laboratory stuff.

- A more big update (based on the third update i want to do :) Just to prepare the terrain) and from 
the users requests.

- And the last: An addon to create spaceships with the kit :)


-------------------------------------------------------------------------------------
"Oh my god! there is a missing object! (There is always a missing object in a kit...)
-------------------------------------------------------------------------------------
To adapt a missing object to your kit, feel free to serve you some parts of textures!

Indeed, the kit is made with the rules of the modular design, and the textures also go
to this direction, it is possible, for example, with the texture: 

Wall_Atlas_07_ID.png 

To create (for those who know a 3D software) a calculation server ? or whatever
you want/need? just stay within each of textures and make your object based on them.

Wall_Atlas_01_ID.png

By example is used for 90% of the kit objects mapping.  Because yes!, create a video game, 
takes a bit of work .... 

Believe that a kit can solves all your game design issues for $80, and without working, 
is a heresy :)

A kit is a starting base! I will try one day to make a video to explain an example of 
re-using of the textures to create a needed/missing object.


--------------------
Creepy note for me :
--------------------

