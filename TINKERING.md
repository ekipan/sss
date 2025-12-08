# SSS: The Silent Soviet Stacker

![Log: game and memory commands, savestating, recompiling, etc.](shots/devel.png)

- See the README to [jump in and play][rea].
- **This how-to** takes you from player to tinkerer.
- The [design rundown][des] is my bucket document: a mix of
  tutorial (how), background (why), and reference (what). I
  hope you'll indulge my sloppiness and dig for what you need.
- The [Forth source][sss] is damn dense, as its intended
  audience is just myself. For the adventurous!

[rea]: README.md
[des]: DESIGN.md
[sss]: sss.fs

## Grabbing a Wrench

> [!WARNING]
> No high scores, no menus, and unconventional controls.
> This is a programmer's toybox first, a game second.

Load the [durexForth][dur] cart or disk into [VICE][vic] to
give it a try. [`sss.fs`][sss] is _tested_ with v4 though I
suspect it will work with v5 also.

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/

You'll want the source in a disk file. You can reuse the
durexForth disk or attach a new blank disk from the VICE File
menu. Typical VICE config has Alt-Insert to paste and `~` for
the C64 `←` key:

```forth
8 device   \ or 9, to select disk drive. 8 is default
v sss.fs   \ open durexForth vi-clone editor
i<alt-insert>~ZZ  \ make file, return to Forth
include sss.fs    \ compile program. ~30 seconds, so:
<alt-w>    \ VICE warp speed, and again to turn off
help       \ learn the keys
new        \ play a bit, press space to pause
bg dd      \ clear screen, draw canvas so you can see:
enter dd   \ cheat: move the piece back to top and:
r          \ continue playing, or:
3 shape c! \ cheat: change to a T piece
r          \ continue playing
1 prof r   \ show blue=draw gray=game while framestepping
0 prof     \ turn profiling back off
123 init r \ restart with a fixed seed
v          \ edit the source, maybe save or VICE snapshot
redo       \ ask the program to recompile itself
words      \ see what's available in the dictionary
asdf       \ error: resets the stack for a clean workspace
\ try things! beginners, do check out starting forth!
```

> [!TIP]
> If you see a reverse-video error message like `redo?` then the
> program was probably unloaded. First try `include sss.fs` to
> recompile, then resort to loading a snapshot or resetting
> VICE.

C64 disk operations are painfully slow. I use JiffyDOS and
VICE HFS (below) to cope. JiffyDOS config is out of document
scope but if you're also feeling stifled maybe try:

## Getting More Leverage

<!-- TODO make durexForth github discussion -->

My own workflow developing durexForth programs is
_terribly janky,_ documented here for completeness sake.
It enables me to:

- Store source outside of VICE on the disk of the actual
  computer I'm using, thus:
- Use whatever editor I'm comfortable with. For the record it
  was some Notepad2 fork back on Windows, and KWrite now on
  Linux. Just the first thing I tried and it works fine.
- Have an immediate edit-test cycle:

1. Do some edits in KWrite
2. Ctrl-S Alt-Tab (to VICE)
3. "redo" Alt-W (to warp speed compile) Alt-W (off)
4. "r" (or however I want to exercise the new code)
5. Alt-Tab (back to KWrite)

Under VICE Preferences > Settings > Peripheral Devices:

- **Drive:** check "Virtual device" and "IEC device",
  set IEC device type to "Host file system"
- **Host file system device:** set directory.

**CAVEATS:**

1. PETSCII lowercase = ASCII uppercase, but ASCII lowercase =
   not valid PETSCII letters, so source must be UPPERCASE.
2. VICE seems to expect and translate CRLF to CR, sometimes.
   Without the LF, INCLUDE causes VICE to chop off the first
   character of the next line. Some weird interaction between
   VICE and durexForth?
3. Something changed in durexForth v5 and now PARSE-NAME is
   interacting with all this in a way I don't understand.
4. Probably more I can't think of right now.

To cope with 1 and 2 I use:

```ini
# .git/info/attributes
*.fs text eol=crlf filter=petscii-case

# .git/config
[filter "petscii-case"]
	clean = "tr '[:upper:]' '[:lower:]'"
	smudge = "tr '[:lower:]' '[:upper:]'"
```

To cope with 3 I develop with durexForth v4 instead.

I opened [#584] about a year ago, hoping the nagging feeling
would force me to investigate and fix it. I still have not, so
it's about time I close the issue and apologize for wasting
Mr. Kotlinski's time.

[#584]: https://github.com/jkotlinski/durexforth/issues/584

You might think this is a lot of work to avoid storing files
in disk images, and that's fair, but you can't argue with
_Ctrl-S Alt-Tab_!
