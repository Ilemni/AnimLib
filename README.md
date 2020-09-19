# AnimLib

### Terraria mod library to allow for an easier creation of animations

AnimLib is a mod library, meant to be utilized by other mods. This does nothing on its own, but is a framework for other mods to create animations. The purpose of this mod is for other mods to easily create more complicated animations for sprites of their players.

## For Modders

There are two components required for your mod to use this animation framework: [AnimationSource](AnimLibMod/Animations/AnimationSource.cs) and [AnimationController](AnimLibMod/Animation/AnimationController.cs). You will also have to actually draw your sprites using tML's PlayerLayers.

---

### [AnimationSource](AnimLobMod/Animations/AnimationSource.cs)

`AnimationSource` is the database for your animations, such as tracks. Your `AnimationSources` are constructed by `AnimLibMod` during startup (`Mod.Load()`), and a single instance of it will exist at any given time. You can have multiple classes derived from `AnimationSource`.

`AnimationSources` consist of 2 abstract properties: `spriteSize` and `tracks`
- `spriteSize` is simply the size of all sprites. In a spritesheet, all sprites are expected to be the same size.
- `tracks` stores all animation tracks that you can use. These determine which sprite to use, as well as how long a frame plays for. For more information, read further below in the **Tracks** section.

The spritesheet texture is assigned in `AnimationSource.Load(ref string texturePath)`, similarly to how tML's ModItems and other Mod classes determine their texture. Although this assigns the main spritesheet texture, you are able to have your Tracks use a different spritesheet instead, if necessary.

There is one "main" animation at a given time, that is, how long frames are and how long the track is for the `AnimationController` depends on which `Animation` is the main animation. This is accessed through `AnimationSource.MainAnimation`, and changed through `AnimationSource.SetMainAnimation()`

If you wish to access your AnimationSource instance, you can get it with `AnimLibMod.GetAnimationSource<MyAnimationSource>()`;
- AnimLib creates these during `Mod.PostSetupContent()`, so this method cannot be used during this time.

---

### [AnimationController](AnimLibMod/Animations/AnimationController.cs)

`AnimationController` is the controller for all animations, and stores current animation data for the player. This also controls how animations are played. Your `AnimationControllers` types are collected by `AnimLibMod` and are constructed during player initialization. There exists one instance per player. You can only have one class derived from `AnimationController`.

`AnimationController` has one abstract member, and that is `Update()`
- `Update()` is where you will put the logic for choosing what track is played. In here you will make one call per `Update()` loop to the method `IncrementFrame()`. For this you will specify the track to play. If the track is the same as it was previously, the track plays normally.
- Example code for `Update()` can be found in the Xmldoc for [AnimationController.Update()](AnimLibMod/Animations/AnimationController.cs).
- For a more advanced example, see the [OriMod's implementation of AnimationController](https://github.com/TwiliChaos/OriMod/blob/ddae89ac101a33067ceb218e3463a9b4a198e77e/Animations/OriAnimationController.cs#L42).

---

### [Animation](AnimLibMod/Animations/Animation.cs)

`Animation` is the glue between your `AnimationController` and `AnimationSources`. In each `AnimationController` there is one `Animation` per any `AnimationSource` you have. These are created automatically for your `AnimationController`.

`Animation` has various properties that you may use, that represent the current Animation.
- `CurrentTrack` is the `Track` in the `AnimationSource` that your `AnimationController` is currently playing.
    - If the `AnimationController` is not playing a track that is in `AnimationSource`, this will return the first track.
- `CurrentFrame` is the `Frame` in the `CurrentTrack` that your `AnimationController` is currently playing.
    - If the `AnimationController`'s current frame index is out of bounds for this `AnimationSource`, this will return either the first or last frame based on the index.
- `CurrentTile` represents a `Rectangle` of the `CurrentFrame`, and map to your spritesheet. Values are in pixels.
- `CurrentTexture` is the `Texture2D` in the `AnimationSource` that should be drawn.
    - For the most part this is the one in `AnimationSource`, but changes if `CurrentTrack` has a different texture to play instead.

`Animation` also contains some helpful methods.
- `GetDrawData(PlayerDrawInfo)`: This gets you a `DrawData` with a bunch of stuff already set up for you. Feel free to change the values in the returned `DrawData` if you need to, such as color.
- `TryAddToLayers(...)`: This simply checks if the current track playing in `AnimationController` is also a track in `AnimationSource`, before adding or inserting the layer into the list.

---

### Creating a [Track](AnimLibMod/Animations/Track.cs)

`Track` construction should happen in `AnimationSource`. `Tracks` contains an array of [Frames](AnimLibMod/Animations/Frame.cs) that determine which sprite is drawn, for how long, and even which spritesheet texture is used. A Track is constructed using an array of `Frame`s, and optionally a `LoopMode` and `Direction`.
- [LoopMode](AnimLibMod/Animations/LoopMode.cs) is what your `AnimationController` will do when it reaches the last frame. The animation will either stay on that frame indefinitely (`LoopMode.None`), or go back to the start (`LoopMode.Always`). By default, this is `Always`.
- [Direction](AnimLibMod/Animations/Direction.cs) is the direction that your `AnimationController` will play the animation. The track can play forward (`Direction.Forward`), backwards (`Direction.Reverse`), or alternate between the two (`Direction.PingPong`). To use `PingPong`, `LoopMode.Always` must also be used.

`Track` construction can take either a `Frame[]` or `IFrame[]`. There's two important differences here.
- A `Frame[]` is simply used as is. This track is intended to use up to one texture.
- An `IFrame[]` should only be used if you will include `SwitchTextureFrame`s. These are `IFrames` specifically designed to allow switching spritesheets during a `Track`. The texture is added to the `Track`, and the `SwitchTextureFrame` is converted to a regular `Frame`. This should only be used if your `Track` will switch textures mid-frame.

[Frames](AnimLibMod/Animations/Frame.cs) represent one frame on the spritesheet. This contains the X and Y position of the frame (in sprite-space), as well as the duration. The duration is optional, and the default value is 0, where the track does not advance.

Frame construction can be shorthanded during Track construction. Instead of using a bunch of
- `new Frame(0, 0, 10), new Frame(0, 1, 10), ...`

You can use a shorthand method `F(x, y, duration)`. So a Track creation can look like
- `F(0, 0, 10), F(0, 1, 10), ...`

If a Track is using a range of frames in a line, and they all play for the same duration, this can be even shorter. Let's say an animation consists of 10 frames. Instead of using
- `new Track(new[] {F(0, 0, 10), F(0, 1, 10), F(0, 2, 10), ... F(0, 9, 10)})`

You can use the method `Track.Range()`. So a Track creation can look like
- `Track.Range(F(0, 0, 10), F(0, 9, 10))`

If a `Track` consists of only one `Frame`, use `Track.Single(Frame)`

---

### Drawing the Animation

Although animation stuff is handled (mostly) automatically, you still need to use `ModifyDrawLayers` to render the animation yourself. This is because you may have specific requirements to draw the player, such as disabling the vanilla sprite's body. If you're familiar with `PlayerLayers` and `ModPlayer.ModifyDrawLayers()`, great. If not, either Google, ask in the tML Discord server, or try to make sense of [OriMod's implementation](https://github.com/TwiliChaos/OriMod/blob/ddae89ac101a33067ceb218e3463a9b4a198e77e/OriLayers.cs#L18).

The simplest way to get a `DrawData` to draw is from `AnimLibMod.GetDrawData`.

    internal readonly PlayerLayer MyPlayerLayer = new PlayerLayer("MyMod", "MyPlayerLayer", delegate (PlayerDrawInfo drawInfo) {
      DrawData data = AnimLibMod.GetDrawData<MyAnimationController, MyAnimationSource>(drawInfo);
      
      Main.playerDrawData.Add(data);
    };

A more performant way would be to cache your AnimationController in your ModPlayer, and cache your Animation in your AnimationController during its initialization. So your DrawData code would look something like this

    MyModPlayer modPlayer = drawInfo.drawPlayer.GetModPlayer<MyModPlayer>();
    DrawData data = modPlayer.myAnimationController.myAnimation.GetDrawData(drawInfo);

---

## Q/A

### **Q:** Can this support multiple mods at once?

**A:** AnimLib was designed for multiple mods to take use of it, however, multi-mod functionality is currently untested. It should work, it might not.

### **Q:** Can this be used for NPCs, such as bosses?

**A:** Currently, no.

### **Q:** I want to use more than one spritesheet for my animation

**A:** There are a few approaches to this

- If you can fit all of your sprites for the `AnimationSource` on a single 2048x2048 or smaller image instead, do that instead.

- If a track can fit on its own 2048x2048 or smaller texture, put that track's sprites on one image and use `new Track(...).WithTexture("MyMod/Animations/MyOtherTexture")

- If a track cannot fit in a 2048x2048 texture, use the Track constructor that takes an `IFrame[]`, and use `new SwitchTextureFrame()`, or the shorthand method `F(texturePath, x, y, duration)`

### **Q:** I want to use this mod. My mod uses multiple transformations, but you only allow one `AnimationController`.

Use `AnimationController.SetMainAnimation` to change your animation to a different `AnimationSource`.

### **Q:** Why are Frame values in bytes, and the duration as ushort?

Memory usage was an important consideration for this mod. By nature of being a mod that may have to co-exist with hundreds of others, the less memory used, the better. With Frames like this, a single Frame only takes 4 bytes, so a hundred frames takes 400 bytes, rather than 1200 from using all ints. Additionally, there is no need to store values larger than what is used.
- Frame position is in sprite-space. If a spriteSize in an `AnimationSource` is 128x128, a frame of, say, \[1,4\] is positioned at 128,512. Coupled with how the max texture size is 2048x2048, this is only an issue if sprites are 8 pixels or smaller, *and* there needs to be more tha 65535 sprites for that 8 pixel character. In that case a second spritesheet could be used.
- Frame duration is in frames (the time...), so the max value would be, at worst, 18 minutes. If a frame needs to be longer than 18 minutes (dear god why), IncrementFrame accepts an int value for overriding it.