# SSS: The Silent Soviet Stacker

A block-stacking game for the Commodore 64 written in the
number-stacking language Forth. It's optimized strictly for my
own joy, which means a 50fps target, forgoing sound, and as
little assembly code as necessary, because Forth is fun!

<table><tr>
  <td><img alt="A block falling into the well"
    src="https://imgur.com/YcWYiBM.png"
    style="max-width: 100%; height: auto;"></td>
  <td><img alt="Log of game and memory commands"
    src="https://imgur.com/Ykam6xL.png"
    style="max-width: 100%; height: auto;"></td>
</tr><tr>
  <td align="center"><em>Game in progress</em></td>
  <td align="center"><a href="#file-3-devel-txt"><em>Live coding session</em></a></td>
</tr></table>

## Trying It Out

It's probably easiest to load the [durexForth][dur] cartridge
or disk into a [Commodore 64 emulator][vic] then paste
[the program][src] directly. You could also get it onto a disk
file then type `include sss.fs`. durexForth has a vi clone if
you want to make a file that way:
`v sss.fs<return>i<alt-insert>~ZZ` to paste, save, and exit in
the usual VICE config.

After compiling, type `help` to see keys, `new` to play,
`1 prof 123 init r` then framestep to [measure time][per].

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/
[per]: #performance-and-tradeoffs

## How it Works

The [design tour][des] explains much of the nitty-gritties.
Cross-reference it with [the source][src].

[src]: #file-2-sss-fs
[des]: #file-1-design-md

<!-- end of README.md -->

