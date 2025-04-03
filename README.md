# AnimLib

### Terraria library mod to allow for an easier creation of complex animations

AnimLib is a library mod, which serves as a framework for other mods to create complex animations.

## For Modders

This mod requires the usage of Aseprite files for character animations.
Since this mod depends on Aseprite's animation tags, layers, and userdata, PNG files are not supported.

## This readme is out of date, and requires a full rewrite.

Some of the classes references below no longer exist, and have been replaced by more versatile systems.

Outdated information below:

---

## Animations

All animations that are handled by this mod are to be stored in
[**Aseprite**](https://github.com/aseprite/aseprite) files.
The Aseprite file is imported and processed into an AnimSpriteSheet asset, accessed with `ModContent.Request`.
The animation logic is handled inside an [AnimationController](AnimLib/Animations/AnimationController.cs).
The sprites are drawn using tModLoader's PlayerDrawLayers.

---

### [AnimSpriteSheet](AnimLib/Animations/AnimSpriteSheet.cs)

[`AnimSpriteSheet`](AnimLib/Animations/AnimSpriteSheet.cs) is a readonly class that your Aseprite file (\*.aseprite|\*.ase) will be parsed to.
This class contains `AnimTag`s and `AnimTextureAtlas`es.
The `AnimTags` may be indexed by Animation Tag name.
The `AnimTextureAtlases` are indexed by layer name.

- [`AnimTags`](AnimLib/Animations/AnimTag.cs) represent [Aseprite's Animation Tags](https://www.aseprite.org/docs/tags/)
In the Aseprite file, these are how you define individual animations,
and they contain information for each frame, such as the frame's sprite sheet index, and frame durations.

- [`AnimTextureAtlases`](AnimLib/Animations/AnimTextureAtlas.cs) contain a reference to a Texture2D asset,
and an array of `Rectangles.`
  - The Texture2D is a spritesheet generated from the animation. Any duplicate or empty frames are merged together.
    - Generally you don't need to know what your spritesheet looks like.
  - Each `Rectangle` is a SourceRect that can be used in a DrawData. The index of the Rectangle corresponds to a frame of animation.
    - Duplicate frames or empty frames will have the same Rectangle values.

The generation of these assets are handled by AnimLib.

**Notes:**
- AnimLib supports automatically upscaling your sprite, so you can work in 1x1 pixels and the mod will upscale them
to Terraria's 2x2 pixels. To allow automatic upscaling, include the string "upscale" in your Sprite UserData.
  - This method of enabling the feature may be reworked in the future.
- AnimLib can generate multiple spritesheets from the same Aseprite file.
  - Each root layer is treated as a separate sprite to process to a spritesheet. 
  - Each group layer will have all children merged into it. 
  - All layers with a UserData color of <font color="6acd5b">Green</font> will always be imported as a root layer, even if it is hidden.
    - If they are a child, they won't be merged into their parent. 
  - All layers with a UserData color of <font color="fe5b59">Red</font> will be ignored during import, even if it is visible. 
    - If they are a group layer, all children will also be ignored.
  - The path for each imported layer is "Path/To/LayerName".
    - Usually the path is just the layer name. A Green UserData layer that was a child will have a path.

Requesting an Aseprite file is similar to requesting a Texture2D:

```csharp
var mySpriteSheet = ModContent.Request<AnimSpriteSheet>("MyMod/Path/To/MyAsepriteFile");
```

---

### [AnimationController](AnimLib/Animations/AnimationController.cs)

`AnimationController` is the controller for all animations. This inherits from ModType, and are loaded automatically,similarly to ModPlayers.

This class controls how animations are played, and stores current animation data for the player.

There exists up to one `AnimationController` instance per player per mod.
You can only have one class derived from `AnimationController` at this time.

- `Initialize()` is where you will register `Animations` (see below)

- `Update()` is where you will put the logic for choosing what track is played.
In here you will return an `AnimationOptions`. If the name is the same as it was previously, the tag plays normally.

For a working example of an AnimationController,
see [OriMod's implementation](https://github.com/Ilemni/OriMod/blob/8e4e61bdd2ef6ede944c58a6c8426bf0445cf9c0/Animations/OriAnimationController.cs).

Sample AnimationController:

```csharp
internal sealed class MyAnimationController : AnimationController {
    public Animation PlayerAnim { get; private set; }
    
    public override void Initialize() {
        const string asepriteFilePath = "MyMod/Path/To/AsepriteFile";
        AnimSpriteSheet spriteSheet = ModContent.Request<AnimSpriteSheet>(asepriteFilePath, AssetRequestMode.ImmediateLoad).Value;
        
        PlayerAnim = RegisterAnimation(spriteSheet);
    }
    
    public override AnimationOptions Update() {
        if (Math.Abs(player.velocity.X) > 0.1f) {
            float playerSpeed = Math.Abs(Player.velocity.X);
            return new AnimationOptions("Running", speed: playerSpeed * 0.5f);
        }
        if (player.velocity.Y != 0) {
            string tag = player.velocity.Y * player.gravDir > 0 ? "Jumping" : "Falling";
            return new AnimationOptions(tag);
        }
        return new AnimationOptions("Idle");
    }
}
```

---

### [Animation](AnimLib/Animations/Animation.cs)

`Animation` is the glue between your `AnimationController` and `AnimSpriteSheet`.
These are created in your`AnimationController` when you register them.

`Animation` has various properties that you may use, that represent the current Animation.
- `CurrentTag` is the `AnimTag` in the `AnimSpriteSheet` that your `AnimationController` is currently playing.
- `CurrentFrame` is the `AnimFrame` in the `CurrentTag` that your `AnimationController` is currently playing.
- `GetRect(string)` returns a `Rectangle` at `CurrentFrame` for the spritesheet with the provided layer name. 
  This may be used directly as a SourceRect.
- `GetTexture(string)` returns the `Texture2D` of the spritesheet that should be drawn.
- `GetDrawData(PlayerDrawInfo, layer)` returns a new `DrawData` with a bunch of stuff already set up for you, 
  such as the Texture and SourceRect.
  - Feel free to change the values in the returned `DrawData` if you need to, such as color.

---

### Drawing the Animation

Although animation stuff is handled (mostly) automatically, you still need to use `PlayerDrawLayers` to render the
animation yourself. This is because you may have specific requirements to draw the player,
such as disabling the vanilla sprite's body.
If you're familiar with `PlayerDrawLayers`, great.
If not, either Google, ask in the tML Discord server, or try to make sense of
[OriMod's implementation](https://github.com/Ilemni/OriMod/blob/8e4e61bdd2ef6ede944c58a6c8426bf0445cf9c0/OriLayers.cs).

Sample PlayerDrawLayer:

```csharp
internal sealed class MyPlayerLayer : PlayerDrawLayer {
    protected override void Draw(ref PlayerDrawSet drawInfo) {
        // Make sure it's your own mod's ModPlayer
        MyModPlayer myModPlayer = drawInfo.drawPlayer.GetModPlayer<MyModPlayer>();
        
        // AnimCharacter contains both your AnimationController and AbilityManager
        AnimCharacter animCharacter = myModPlayer.GetAnimCharacter(); // Extension method
        
        // The animation currently in use
        Animation animation = animCharacter.AnimationController.MainAnimation;
        
        // Most of the DrawData is already set up, but some tweaks may be desirable
        DrawData data = animation.GetDrawData(drawInfo, "MyAsepriteLayer"); // The layer name as in the Aseprite file
        
        // Make any tweaks to your DrawData here, e.g. color, in-world offset, shaders
        
        drawInfo.drawCache.Add(data);
    }
    // Other methods
};
```

A more performant way could be to cache your AnimationController in your ModPlayer.
So your DrawData code may instead look something like this:

```csharp
    MyModPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<MyModPlayer>();
    DrawData data = modPlayer.myAnimationController.MainAnimation.GetDrawData(drawInfo);
```

---

## Abilities

AnimLib supports Abilities, actions that your player can take, which are unrelated to items.
These can be as simple as crouching, or other visual actions meant for animating,
or as complex as [OriMod](https://steamcommunity.com/sharedfiles/filedetails/?id=2879483545)'s abilities.

[TODO: Fill in this section]: #

---

## Compatibility

This mod seeks to be compatible with other mods which transform the player in different ways.
The mod will automatically disable animations and abilities if a supported mod's player transformation is active.
At this time, compatibility is limited to [a few mods](AnimLib/Compat/DefaultTweaks).

---

## Q/A

### **Q:** I don't have Aseprite. Can I use another program?

**A:** AnimLib only supports Aseprite files. Purchase Aseprite, or search how to compile it yourself.

### **Q:** Can this support multiple mods at once?

**A:** AnimLib was designed for multiple mods to take use of it, however, multi-mod functionality is currently untested.
It should work, it might not.

### **Q:** Can this be used for NPCs, such as bosses?

**A:** Currently, no.

### **Q:** I want to use more than one spritesheet for my animation.

**A:** Each spritesheet may exist on one layer in the Aseprite file.

### **Q:** I want to use this mod. My mod uses multiple transformations, but you only allow one `AnimationController`.

Use `AnimationController.SetMainAnimation` to change your animation to a different `AnimSpriteSheet`.

### **Q:** I want my player's arm to rotate when the player is using an item.

Currently not supported. A workaround may be to have the arm(s) be on separate layers, and use a different animation
for that arm layer when an item is being used.
