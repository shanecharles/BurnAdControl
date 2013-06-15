# Burn Ad Control

### A wrapper for the Microsoft Advertising library for Windows Phone

This will setup and manage the ad control in your windows phone app page. This 
wrapper was created to alleviate an intermittent null reference error in multi-page 
windows phone apps when navigating.

This now has the capability to show your own ads when the ad server fails. 
Your ad will display for 30 seconds and then try the ad server again. You can 
also show an app highlight when you open the app. The highlight will show for 
10 seconds before attempting the ad server.

Currently the ad control is created at the bottom of the page.