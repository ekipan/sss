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
number-stacking language Forth. **This thing:**

- **Is a personal project** optimized strictly for **my own
  joy,** which means a 50fps target, [forgoing sound][sou],
  and as little assembly code as necessary, because Forth is
  fun!
- **Explores creative constraints.** durexForth is
  _fast,_ but not as fast as hand-rolled 6502. Arbitrary
  [source text constraints][den], too. They're my muse.
- **Wants to be [tinkered with][dip],** so:
- **Might motivate the Forth- or C64-curious,** but:
- **Is NOT a Tetris product!** No sound, no menus, weird keys.
  General audiences will balk, but I play until Game Over to
  pass the time.

<!--
I see you there, reading the raw markdown source. Yeah, TTC
are touchy about their mark, so ya gotta be real careful when
you say "Tetris." We all know this. It's better left unsaid.

But I do mean it earnestly as well: this thing doesn't try to
be a slick product anyone would want to play. It's my toybox.
Come, play with me.
-->

## Trying It Out

1. Load the [durexForth][dur] cart or disk into [VICE][vic].
2. Paste [`sss.fs`][sss] contents.
3. Type `help` then `new`.

[dur]: https://github.com/jkotlinski/durexforth
[vic]: https://vice-emu.sourceforge.io/

### Keys

- **S D F** - Shift left, soft drop, shift right.
- **J K** - Rotate left and right.
- **L** - Exchange piece with hold slot on bottom right.
- Anything else - Pause game and return to Forth.

My own strong preference. See [input rationale][inp] for a(n
admittedly not easy) reconfiguration option.

## How it Works

The [design rundown][des] explains most of the nitty-gritties,
including:

- A guide to [start tinkering][dip], then
- A [deep dive][div] through the core,
- A whirlwind [game tour][gam] and [devtools tour][dev] to
  cross-ref with [the source][sss],
- A [performance report][per], honestly that bigass document
  probably tries to serve too many audiences.

<!-- these currently work in gist. TODO migrate eventually. -->
<!-- [sss]: sss.fs -->
[sss]: #file-2-sss-fs
[des]: #file-1-design-md
[dip]: #dipping-your-toes
[div]: #diving-in
[gam]: #touring-the-rest-part-1-game-stuff
[dev]: #touring-the-rest-part-2-dev-stuff
[per]: #performance-and-tradeoffs
[inp]: #input
[sou]: #sound
[den]: #density

<!-- end of README.md -->

