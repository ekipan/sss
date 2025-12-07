# SSS: The Silent Soviet Stacker

<table><tr>
  <td><img alt="A block falling into the well"
    src="https://imgur.com/YcWYiBM.png"></td>
  <td><img alt="Log: game and memory commands, savestating, recompiling, etc."
    src="https://imgur.com/Ykam6xL.png"></td>
</tr><tr>
  <td align="center"><em>Game in progress</em></td>
  <td align="center"><a href="#file-3-devel-txt"><em>Live coding session</em></a></td>
</tr></table>

A block-stacking game for the Commodore 64 written in the
number-stacking language Forth. This thing is:

- A **personal project** optimized strictly for **my own
  joy,** which means a 50fps target, forgoing sound, and as
  little assembly code as necessary, because Forth is fun!
- A **constrained creative exploration:** durexForth is
  _fast,_ but not as fast as hand-rolled 6502. Arbitrary
  [source text constraints][den], too. They're my muse.
- **NOT a Tetris product!** No sound, no menus, weird keys.
  General audiences will balk, but I play until Game Over to
  pass the time.

## Trying It Out

1. Load the [durexForth][dur] cart or disk into [VICE][vic].
2. Paste [`sss.fs`][src] contents.
3. Type `help` then `new`.

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/

### Keys

- **S D F** - Shift left, drop, shift right.
- **J K** - Rotate left and right.
- **L** - Exchange piece with hold slot on bottom right.
- Anything else - Pause game and return to Forth.

My own strong preference. See [input rationale][inp] for a(n
admittedly not easy) reconfiguration option.

### Tinkering

You'll want the source in a disk file. Either reuse the
durexForth disk or attach a blank disk from the VICE File
menu. Typical config has Alt-Insert to paste and `~` for the
C64 `←` key:

```forth
8 device   \ or 9, to select disk drive. 8 is default
v sss.fs   \ open durexForth vi-clone editor
i<alt-insert>~ZZ  \ make file, return to Forth
include sss.fs    \ compile program
help       \ learn the keys
new        \ play a bit, press space to pause, then:
1 prof r   \ then framestep to measure time
0 prof     \ profiling off
123 init r \ restart with a fixed seed
v          \ edit the source, maybe save again
redo       \ ask the program to recompile itself
words      \ see what's available in the dictionary
\ etc etc
```

Check the [Forth learning resources][spe]. If you see `redo?`
in reverse text, the program was unloaded. Do `include sss.fs`
again to get it back. See elsewhere for info about the
["measure time"][per] thing above.

## How it Works

The [design tour][des] explains much of the nitty-gritties.
Cross-reference it with [the source][src].

<!-- TODO proper links after migrating from gist. -->
[src]: #file-2-sss-fs
[des]: #file-1-design-md
[spe]: #spec-and-background
[per]: #performance-and-tradeoffs
[inp]: #input
[den]: #density

<!-- end of README.md -->

