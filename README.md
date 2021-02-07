# Death Count
Death Count plugin for Advanced Combat Tracker. Lists, counts, and alerts for EQII deaths as they occur.
Features:
* Provides a mini overlay window that can float over the EQII game in windowed mode. 
* Provides a list of incoming damage just prior to death.
* Provides an audio alert upon a death.
* Time filters audio alerts so that a raid wipe does not result in 24 alerts talking over each other.
* Provides an audio alert when the death count reaches the user defined death limit in the **Tracked Names**.

The death count will typically not be exactly correct. 
The plugin cannot distinguish between a player death and class pet death, 
so pet deaths are included in the list. 
A death may be removed from the list by right-clicking it.

Also, if an enemy mob has a one-word name and that name is not in the **Tracked Names** list,
the enemy's death will be added to the death list and an audio alert will be generated.

### User Interface
The majority of the user interaction will likely be with the overlay.
Example of the death list overlay:
![Mini Example](images/deathlist.PNG?raw=true)

Double click of a player name in the list opens a window showing the damage to that
player prior to their death. 
Lines prior to the death are added or subtracted when the window is re-sized. 
"T-0" means damage occurred in the same second as the death. 
"T-1" is one second before death, etc.

![Log Example](images/death_data.PNG?raw=true)

### Setup
Plugin setup is accomplished on the ACT plugin tab. Here the user can select how
the audio and visual alerts occur and establish warning levels for particular mobs.
Help is provided in the bottom pane when the mouse is positioned over a control.
![Plugin](images/plugin.PNG?raw=true)



