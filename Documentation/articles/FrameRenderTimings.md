# Frame Render Timings

If you are reading this, you probably care about exactly when something is shown on the screen (or you're just a curious soul).
Well then, here is a quick introduction to the pain that is figuring out that information for your own setup.

## TL;DR

1. A photodiode, using vsync, and accounting for the 'pixel location timing offset' will give you the best results; however, if that is not possible (or you don't care about sub-ms timings) then keep going down this list.
1. Get an OLED monitor if you care about timing at all.
    - Get a gaming monitor for even better results (ex: ASUS ROG Swift OLED PG27AQDM - updated on 2025-03-28)
    - Maybe get a CRT for even better results with some potential experiment risk
1. High frame rate monitors (240Hz) are better, but only if your computer is powerful enough to keep up.
1. Keep VSync on in your Unity game and your OS (ex: MacOS).
1. Turn on "logFrameDisplayTimes" in your config and post-hoc replace the display events's timestamps with the next "frameDisplayed" event timestamp.
1. If a 16.67ms accuracy (for a 60Hz monitor) or an 8.33ms accuracy (for a 120Hz monitor) is good enough for you, you can stop here. Otherwise, time to get reading.

## But isn't it super simple?

- Can't I just log when I change the GameObject in Unity?
  - The short answer is: no.
  - The long answer is: Why the hell is this so complicated!!!
  - More seriously, unless you use GSync or FreeSync (which we will talk about much later) then the image on screen only changes X times a second. The 'X; is your monitor refresh rate (like a 60Hz, 120Hz, or 144Hz monitor) and it determines how many times a second your screen changes what it is showing. The time that you change the item in unity is NOT the time that the item actually changes on the screen. VSync helps to solve this issue.
- What is that VSync thing that you mentioned?
  - VSync is a tool that syncs your game with the monitor refresh rate. In other words, your game will only tell the screen to show new things at the same rate the monitor shows new things. This actually sets how often the Update() function is called in Unity (assuming your game isn't lagging).
  - If you have VSync off, you can actually show parts of multiple frames at one time. For example, the top of your screen may show one frame and the bottom of the screen shows the next frame. These lines between different frames being shown at the same time are called "tear lines" (tear as in ripping, not like crying). The general term is "screen tearing".
  - There can be more than 2 frames on screen at once without VSync. There could be 3, 4, or even more.
  - Turning VSync off will also make some of the calculations later harder or impossible.
- Okay, then can't I just log when those VSync timings occur? Just log at the end of each frame.
  - Now you're on the right track. As a matter of fact, PsyForge already does this for you. If you set "logFrameDisplayTimes" to true in your config, then the game will generate a "frameDisplayed" event for the end of each frame.
  - PsyForge even goes one step further and only logs a "frameDisplayed" event when another event has been sent to the log. This saves you the headache of so many "frameDisplayed" log messages for frames you don't care about anyway.
  - Now you just need to replace the timestamp of all events where you changed what was on screen screen with the timestamp of the next "frameDisplayed" event in your log file (session.jsonl). That's because the "frameDisplayed" event is when those display events actually occured.
- So we're done right?
  - Well how accurate do you have to be? And how fast is your monitor?
  - If you have a 60Hz monitor and a 16.67ms accuracy is good enough for you, then you're all set!
  - If you have a 120Hz monitor and an 8.33ms accuracy is good enough for you, then you're all set!
  - The pattern is to caculate: $1 / refresh\_rate$
- Is it really that simple?
  - Heck no!
  - There are plenty more things that throw those theoretical numbers off:
    1. Lag while playing the game
    1. OS or graphics card level double/tripple buffering
    1. Delay between the graphics card vsync pulse and writing the data to the monitor
    1. Monitor pixel change times
- So you decided that you want more accurate timing...
  - Then onto the next section you go.

## So your "frameDisplayed" event isn't even exact?!?

- You are right. Time to learn about monitors.

## An Intro to Monitors (Part 1): The Basics

- There are a 2 main steps in a monitor's output.
  1. Active Display - when the screen is actually changing
  1. VBlank - when the image data for the screen is being delivered to the monitor (ex: over an HDMI cable)
      - There are other things that happen during VBlank, but they are not important to us for this purpose.
- So what we want is the exact time that the active display section starts (or whenever VBlank ends)
  - This is why a photodiode is so nice. If you put it on the top left of the screen, you will get the exact time when that pixel changes for that frame (or at least approximately since you are actually waiting on more than one pixel to change)

## So why don't we just log when the Active Display starts or VBlank ends?

- The simple answer, because I haven't implemented it yet. I can't wait until I can delete this section.
  - Temporary note: This is literally in progress right now.
- This longer answer is that it is way more difficult than it seems because you can't do it in Unity. Not only is it different for every Operating System (Linux, Windows, MacOS, Android, iOS, VR, etc.), but it differs with what graphics card you are using (Nvidia, AMD, integrated graphics). It even changes based on which display server protocol you are using in Linux (X11, Wayland). And to top it all off, sometimes it even works differently between monitors for specific operating systems (looking at you MacOS).
- But, you guessed it, that's STILL not enough!

## An Intro to Monitors (Part 2): Pixels don't change right away?!?

- That is correct, the real world is messy and pixels do not change color right away.
- There is still a slight delay that occurs as the pixels try to change colors. This can either be measured in black-to-white (BTW) or grey-to-grey (GTG) response times.
  - Different types of monitors have different black-to-white response times.
    - LCD is about 10ms BTW and 1-10ms GTG
    - LED is about 10ms BTW and 1-10ms GTG
    - QLED is about 4-5ms BTW and 4-5ms GTG
    - OLED is about 0.5ms-2ms BTW and 0.5ms GTG
  - As you can see, you will want an OLED monitor
- There is also a delay that occurs from transfering data from the graphics card to the monitor itself called "input lag".
  - To spare you the list, OLED is best at this too.
- I recommend you look these values up before buying a monitor. The two best websites are [RTINGS](rtings.com) and [TFT](tftcentral.co.uk)
  1. Check that the Response Time graphs show low values (especially in the top row and the left column)
  1. Input Lag section shows a small value
- These are both relatively constant for each monitor, so it is easy enough to account for with a constant temporal offset.
  - You can either find the values online (RTINGS - input lag) or use a camera to find it for your specific experiment (or even the specific part of the experiment)
  - Then add the delay to your display timestamps

## Can I just use a photodiode sync pulse?

- Absolutely! And it is WAY better than the above! It tells you exactly when the pixels under your photodiode change.
- Side notes about photodiodes
  - Don't forget when using a photodiode to take into account how long it takes to actually SEND the data from the photodiode controller to your app and timestamp it. That delay could make a difference (usually this is negligible and is less than 1ms).
- Downsides of photodiodes
  1. You can't actually change it every frame. So this will only work for specific events that don't happen too rapidly in succession, not for every frame that is logged like the "frameDisplayed" event. So if you have some things changing on screen very fast, consider your photodiode setup carefully.
  1. You can't use it in VR headsets. Although you may be able to use it to figure out what your constant offset from your log events should be and then hope the game doesn't lag.
- But, yet again, it still doesn't get you all the way there... Not all the pixels on a monitor change at the same time.

## An Intro to Monitors (Part 3): Not every pixel shows up at the same time?!?

- Right again! Not every pixel on a monitor is updated at the same time. As a matter of fact, they only update one at a time.
- Monitors actually place them from left to right and top to bottom (like you are typing a word document).
- This means, if you want to know when a specific pixel appears on the screen, you will have to calculate when it should appear.
- First you have to find the following things:
  - Your screen resolution (ex: 1920x1080)
    - horizontal pixels (1920)
    - vertical pixels (1080)
    - This assumes you fullscreen your game
  - What horizontal and vertical pixel you care about on the screen
    - If you don't know how to do that then follow these steps: download [GIMP](https://www.gimp.org/), open the file, mouse over where you care about, and then look at the bottom left where it shows your X (horizontal) and Y (vertical) coordinates.
  - Your monitor refresh rate (ex: 120Hz)
- We are going to assume your photodiode is in the top left corner for the following exammple. This means that the photodiode pixels turn on first, at the start of the frame.
- To find when the pixel will display after the start of the frame, it would be:
  $$\frac{horizontal\_pixels * Y + X}{horizonal\_pixels * vertical\_pixels * refresh\_rate}$$
- Example
  - Input values
    - pixel coordinate $X = 960$ (halfway across the screen)
    - pixel coordinate $Y = 540$ (halfway down the screen)
    - $refresh\_rate = 120Hz$
    - $horizontal\_pixels = 1920$
    - $vertical\_pixels = 1080$
  - Then the pixel you cared about would display 4.17ms after the start time of the frame.
  $$\frac{1920 * 540 + 980}{1920 * 1080 * 120Hz} = 0.00417s = 4.17ms$$
- We will call this this 'pixel location timing offset'

## So how do I account for all of this stuff?

- No matter what, make sure VSync is on.

### Photo Diode Method

- This is definitely the easiest. Just account for the pixel location timing offset.
- To be extra precise, try to account for the delay of the photodiode logging as well.

### Frame Displayed Method

- This obviously has a lot more things to account for.
  1. pixel location timing offset
  1. OS level double/tripple buffering
      - Check in your OS settings
      - If you use MacOS, see the [MacOS forced tripple buffering](#macos-forced-tripple-buffering) section
  1. Graphics card double/tripple buffering
      - See this in your graphics card software settings
  1. Unity double/tripple buffering
      - Check for the [QualitySettings.maxQueuedFrames](https://docs.unity3d.com/6000.2/Documentation/ScriptReference/QualitySettings-maxQueuedFrames.html) in your code and set it to how many buffered frames you want. By default, the value is 2, meaning there is 1 extra buffered frame. This would create a delay of 1 frame's time between your "frameDisplayed" event timestamp and the when the gpu actually got the data to write to screen.
  1. Monitor pixel change times
      - See the [Intro to Monitors (Part 2)](#an-intro-to-monitors-part-2-pixels-dont-change-right-away) section.
- How do I actually account for all of them? What is the total offset?
  1. We make the assumption that there is not much lag between the end of vblank and when the graphics card writes to the screen.
  1. When processing your log file, for every event, find the next "frameDisplayed" event, add all of the above sources of delay to the "frameDisplayed" event timestamp, and then replace the event timestamp with that value.
- Simple verification method
  - Required supplies:
    - A phone with 8x slowmo recording (240fps) or faster
    - A little LED light that you can turn on from some C# code
  - Process:
    1. Add something to the experiment that displays to the top left screen.
    1. Add a line to turn on the LED for 100ms in the *PsyForge/Runtime/Scripts/DataMangement/EventReporter.cs* file directly after the *LogTS("frameDisplayed", ...)* line.
    1. Run your game while recording both the screen and the LED with you camera.
    1. Open the video and go frame by frame until you see the thing appear in the top left of the screen.
    1. Now count how many frames you have to go back until the LED initially turned on.
    1. Calculate your total offset using the info you gathered above about your monitor and computer.
    1. Calculate your real offset $number\_of\_camera\_frames * camera\_fps$
    1. Compare your total offset and your real offset. If your real offset is consistently greater/smaller than your total offset by $1/camera\_fps$ then you calculated your total offset wrong.
  - This will tell you, within $1/camera\_fps$ seconds, how accurate your logging is. So if your camera shoots at 240fps, then this will tell you your accuracy within 4ms.

## Extra facts that may make you sad/annoyed

### MacOS forced tripple buffering

MacOS versions 10.13 and later have a forced triple buffering by the OS (you can't turn it off), which means there is always a 1 frame delay between when your task "says" it did a thing and when it actually does get displayed to the screen. This doesn't impact durations or relative timings within the task, but it does impact timings relative to externally collected data (ex: EEG). Thank you PsychoPy for actually discussing this issue publicly [in a basic form here](https://discourse.psychopy.org/t/correcting-for-mac-triple-buffering/15391) and [more in-depth here](https://github.com/psychopy/psychopy/issues/2250).

## What is GSync and FreeSync? Should I use them?

- GSync and FreeSync are the next great innovation in monitor hardware.
- GSync and FreeSync are the same thing, but GSync is for NVidia graphics cards and FreeSync is for AMD cards. For future bullet points I will just call it GSync, but know that everything applies to both GSync and Freesync.
- Remember with VSync how the monitor waits for each frame and displays the whole frame all at once (doesn't write part of a frame to screen), well that comes with a downside. If you do NOT get the frame to ready in time then the next time the monitor can write to the screen is on the next VSync. This effectively halves your monitor refresh rate.
- GSync actually allows the monitor to refresh the screen with the new frame as SOON as your new frame is completely ready. This means that there isn't screen tearing, while avoiding the delays associated with VSync.
- The main benefit is smoother gameplay when there is a slight lag in your computer.
- When calulating the pixel location timing offset with GSync, you still use the normal refresh rate of the monitor for the calculation, because that is how fast the pixels will change.
- NOTE: This only works on GSync and FreeSync certified monitors since this is hardware feature of the new monitors.

## Maybe consider a CRT

- CRTs (you know those old timey HUGE TVs) are actually still better than the best gaming monitors today when it comes to input lag and BTW/GTG response times.
- As a matter of fact, their input lag and their BTW/GTG response times are normally measured in micro-seconds (so nothing).
- There is also a LARGE caviat with CRTs. They don't actually show everything on screen all at once (like modern monitors). Instead, each pixel is only shown for about 5ms before it fades back to black. The brain then sees each of these specks of light and merges them into an image. This can be either advantageous or disadvantageous depending on what you are trying to study in your psychology/neuroscience experiment.
- If you need an HDMI to VGA adapter for your PC, then the industry standard is the "Tendak Active 1080P Female HDMI to VGA Male Converter". It makes sure you keep that nice 0ms input lag.
- Since these things are huge, I recommend buying locally. Most people from your local Retro-Gaming groups will probably have extra CRTs they would be willing to sell.
- Yes, it is possible to get 1080p 60Hz CRTs, but it may be tricky to find.

## For even MORE info, go to these links

- [Understanding Display Scanout Lag](https://blurbusters.com/understanding-display-scanout-lag-with-high-speed-video/)
- [GSync 101](https://blurbusters.com/gsync/gsync101-input-lag-tests-and-settings/)