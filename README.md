# MemeMastersCode

## So what's this?

The following is basically all of the code used for Meme Masters, a free game I made for Android devices (hoping to release to iOS in the coming days)
Note that this is a game made solely by me, with little intention to be read by others, so the code may not be 100% up to standards.
Or in other words...

![Code Review](https://imgs.xkcd.com/comics/code_quality_3.png)

## Where should I look?

There are a lot of files, but here are a few suggestions:
* The initial boot code ( LoadAssetBundles.cs , InitialScript.cs , MemeLoaderScript.cs ) is a pretty good example of my approach to loading assets from internet/cache, in order to ease up the app size.
* Looking at the Shop files (ShopConfirmationScript.cs , ShopItemClass.cs, ShopItemScript.cs, ShopManager.cs) can give you a feel of an enclosed system of code working together.
* For those brave enough to face the terrors of networking match code, the match files (TurnManager.cs and MatchManager.cs) contain the logic of the match playing out, and the data sent to/from the server to its clients
