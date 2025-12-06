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

- A personal project optimized strictly for my own joy, which
  means a 50fps target, forgoing sound, and as little assembly
  code as necessary, because Forth is fun!
- A constrained creative exploration: durexForth is _fast,_
  but not as fast as hand-rolled 6502. Arbitrary
  [source text contraints][den], too. They're my muse.
- **NOT** a Tetris product! No sound, no menus, weird keys.
  General audiences will balk, but I play until Game Over to
  pass the time.

## Trying It Out

1. Load the [durexForth][dur] cart or disk into [VICE][vic].
2. Paste [`sss.fs`][src] contents.
3. Type `help` then `new`.

Another option is to get `sss.fs` onto a disk file somehow
then type `include sss.fs`, enabling `redo` for development.
durexForth has a vi-like editor if you want to make a file
that way. To paste and save in the usual VICE config:

```
v sss.fs<return>i<alt-insert>~ZZ
```

After `help` and `new`, try `1 prof 123 init r` then framestep
to [measure time][per].

### Keys

- **S D F** - Shift left, drop, shift right.
- **J K** - Rotate left and right.
- **L** - Exchange piece with hold slot on bottom right.
- Anything else - Pause game and return to Forth.

My own strong preference. See [input rationale][inp] for a(n
admittedly not easy) reconfiguration option.

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/
[per]: #performance-and-tradeoffs
[inp]: #input
[den]: #density

## How it Works

The [design tour][des] explains much of the nitty-gritties.
Cross-reference it with [the source][src].

[src]: #file-2-sss-fs
[des]: #file-1-design-md

<!-- end of README.md -->

