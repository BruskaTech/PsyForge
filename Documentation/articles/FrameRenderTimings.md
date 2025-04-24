# Frame Render Timings

If you are reading this, you probably care about exactly when something is shown on the screen (or you're just a curious soul).
Well then, here is a quick introduction to the pain that is figuring out that information for your own setup.

## TL;DR

1. Get an OLED monitor if you care about this at all.
    - Get a gaming monitor for even better results (ex: ASUS ROG Swift OLED PG27AQDM - updated on 2025-03-28)
1. Keep VSync on in your Unity game.
1. Turn on "logFrameDisplayTimes" in your config.
    - If you are being extra careful, use a photodiode and read that section
1. Replace the display events' timestamps with the next "frameDisplayed" event timestamp.
1. If a 16.67ms accuracy (for a 60Hz monitor) or an 8.33ms accuracy (for a 120Hz monitor) is good enough for you, you can stop here.
1. 

## But is it super simple?

- Can't I just log when I change the GameObject in Unity?
  - The answer is no, because unless you turn off VSync (which you definitely shouldn't do for other reasons explained below) then the image on screen only changes X times a second. Your monitor refresh rate (like a 60Hz, 120Hz, or 144Hz monitor) determines how many times a second your screen changes what it is showing.
- What is that VSync thing that you mentioned?
  - VSync is a tool that syncs your game with with the monitor refresh rate. In other words, your game will only tell the screen to show new things at the same rate the monitor shows new things. This actually sets how often the Update() function is called in Unity (assuming your game isn't lagging).
  - If you have VSync off, you can actually show parts of multiple frames at one time. For example, the top of your screen may show one frame and the bottom of the screen shows the next frame. These lines between different frames on one screen are called "tear lines" across the screen(tear as in ripping, not like crying). The general term is "screen tearing".
  - There can be more than 2 frames on screen at once without VSync. There could be 3, 4, or even more.
  - Turning VSync off will also make some of the calculations later harder or impossible.
- Okay, then can't I just log when those VSync timings occur? Just log at the end of each frame.
  - Now you're on the right track. As a matter of fact, PsyForge already does this for you. If you set "logFrameDisplayTimes" to true in your config, then the game will generate a "frameDisplayed" event for the end of each frame.
  - PsyForge even goes one step further and only logs a "frameDisplayed" event when another event has been sent to the log. This saves you the headache of so many "frameDisplayed" log messages for frames you don't care about anyway.
  - Now you just need to replace the timestamp of all events about displaying things to screen with the timestamp of the next "frameDisplayed" event in your log file (session.jsonl). That's because the "frameDisplayed" event is when those display events actually occured.
- So we're done right?
  - Well how accurate do you have to be? And how fast is your monitor?
  - If you have a 60Hz monitor and a 16.67ms accuracy is good enough for you, then you're all set!
  - If you have a 120Hz monitor and an 8.33ms accuracy is good enough for you, then you're all set!
  - The pattern is to caculate: $1 / refresh\_rate$
- Well that's not good enough for me...
  - Then onto the next section you go.

## Can I just use a photodiode sync pulse?

- Absolutely! And it is better than the above! But it only get's you halfway there...
- As a matter of fact, it only gets you as far as the last section ended, just more accurately.
- Technically it gets you a TOUCH more accurate timings because there is a difference when the frame ends (what PsyForge is logging) and when the graphics card actually gives the sync signal for the next frame data.
- Don't forget when using a photodiode though to take into account how long it takes to actually SEND the data from the photodiode controller to your app and timestamp it. That delay there can make a difference (usually a few milliseconds).
- Another downside of a photodiode: you won't be informed when the screen does NOT change (and you can't actually change it every frame). So this will only work for specific special events that matter the most, not for every frame that is logged like the "frameDisplayed" event.

## So your "frameDisplayed" event isn't even exact?!? (Part 1)

- You are right. Time to learn about monitors.

## An Intro to Monitors (Part 1)

- There are a 2 main steps in a monitor's output.
  1. Active Display - when the screen is actually changing
  1. VBlank - when the image data to the screen in being delivered to the monitor (ex: over an HDMI cable)
      - There are other things that happen during VBlank, but they are not important to us for this purpose.
- So what we want is the exact time that the active display section starts (or whenever VBlank ends)
  - This is why a photodiode is so nice. If you put it on the top left of the screen, you will get the exact time when that pixel changes for that frame (or at least approximately since you are actually waiting on more than one pixel to change)

## So why don't we just log when the Active Display starts or VBlank ends?

- The simple answer, because I haven't implemented it yet. I can't wait until I can delete this section.
- This longer answer is that it is way more difficult than it seems because you can't do it in Unity. Not only is it different for every Operating System (Linux, Windows, MacOS), but it differs with what graphics card you are using (Nvidia, AMD). It even changes based on which display server protocol you are using in Linux (X11, Wayland).

## An Intro to Monitors (Part 2)

### Pixels don't change right away?!?

- That is correct, the real world is messy and pixels do not change color right away.
- There is still a delay that occurs as the pixels try to change colors. This can either be measured in black-to-white (BTW) or grey-to-grey (GTG) response times.
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
- These are both relatively constant for each monitor though, so it is easy enough to account for.
  - You can either find the values online (RTINGS - input lag) or use a camera to find it for your specific experiment (or even the specific part of the experiment)
  - Then add the delay to your display timestamps

### Not every pixel shows up at the same time?!?

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
  
- Now the 


## So your "frameDisplayed" event isn't even exact?!? (Part 2)

- So if you want the exact time you will have to account for all of that stuff. So let's make an see some equations and write an example.

## Maybe Consider a CRT

- CRTs (you know those old timey HUGE TVs) are actually still better than the best gaming monitors today when it comes to input lag and BTW/GTG response times.
- As a matter of fact, their input lag and their BTW/GTG response times are normally measured in micro-seconds (so nothing).
- There is a limitation with their response times that once they are colored, it takes about 5ms for them to turn black.
  - So if the timing of text matters for your experiment, use white text on a black background.
- If you need an HDMI to VGA adapter for your PC, then the industry standard is the "Tendak Active 1080P Female HDMI to VGA Male Converter". It makes sure you keep that nice 0ms input lag.
- Since these things are huge, I recommend buying locally. Most people from your local Retro-Gaming groups will probably have extra CRTs they would be willing to sell.
- Yes, it is possible to find 1080p 60Hz CRTs, but it may be tricky to find.


https://blurbusters.com/understanding-display-scanout-lag-with-high-speed-video/