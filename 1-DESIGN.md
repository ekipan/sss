<!-- TODO **Will become** true after I migrate to a repo:
> ![NOTE]
> [sss.fs][src] has CRLF line endings because of my wacky
> dev workflow that uses VICE's host filesystem feature not
> technically supported by durexForth.
-->

# SSS: The Silent Soviet Stacker

See the README to [jump in and play][rea].

The [main source][src] is damn dense, as its intended audience
is just myself. This tour, however, aims for less longbearded
folk, with overview, implementation detail, tradeoff
reflections, etc.

[src]: #file-2-sss-fs
[rea]: #file-0-tet-readme-md

## Spec and Background

I've played tons of The Tetris Company (TTC) games, I strongly
admire Tetris The Grandmaster (TGM), plus platform constraints
and my own preferences make for an eclectic mix.

I was personally drawn to the [Commodore 64][c64] _only after_
stumbling on [durexForth][dur]. They're both lots of fun!

Forth is an old and grumpy programming language that I adore.
[Forth on Wikipedia][fow] and the beloved
[Starting Forth][sta] are great places to start.

[c64]: https://www.c64-wiki.com/
[dur]: https://github.com/jkotlinski/durexforth
[fow]: https://en.wikipedia.org/wiki/Forth_(programming_language)
[sta]: https://www.forth.com/starting-forth/

Forth subroutines are called "words" and operate on a stack of
values. I use compact stack comments to fit the cramped C64
screen:

- `erase ( au-)` address, unsigned count -> (no result).
- `piece ( pts-ppppc)` piece position, turn count, shape index
  -> 4 block positions, color code.
- `hit? ( ppppc-f)` 4 positions, (ignored) color code
  -> boolean flag.
- `split ( $yyxx -- $xx $yy )` sometimes I lean closer to
  conventional ANS style when I think the clarity is needed.

## Diving In

The `piece` word is the heart of this program. It computes
block positions from a piece description. Read this
interpreter session closely:

```forth
\ these comments, wider than the screen, added afterward.
hex bg
ok \ sets up number base and screen canvas.
1305 2 1 piece .s \ row 19($13) col 5 turns 2 shape J(1)
1304 1305 1306 1404 8 ok \ 4 blocks + orange(8) color
hit? .
0 ok \ false = no collision
1305 2 1 piece plot
ok \ paints rotated J piece at top of well.
\ though the 1404 above overwrites part of that canvas.
0 0 0 piece .s hit? . \ bottom left(0) flat(0) I(0) piece.
-2 -1 0 1 3 -1 ok \ coords out of bounds (last -1)
```

Packed hex `$yyxx` coordinates exist in three spaces:

- **Blockspace:** `0 <= y <= 3, -2 <= x <= 1` <br>
  `$0000` = piece center to compute blocks and check lines.
- **Wellspace `th-w`:** `0 <= y <= 22, 0 <= x <= 9` <br>
  `$0000` = bottom left of playfield of `land`ed blocks.
- **Screenspace `th-c`:** `0 <= y <= 20, 0 <= x <= 14` <br>
  `$0000` = screen row 22 column 13 near bottom left of main
  21x19 canvas, corresponding to well origin. Hold and next
  queue are on the right `11 <= x <= 14`.

The well extends two rows above screen (21 and 22) for new
pieces (`$1305` = row 19 column 5) to rotate into.

The orange value `8` above can be [`lock`][loc]ed into the 4
well positions if not `hit?`-detected, or `plot`ted on screen.
These use memory indexing `n th` words: `0 th-w` for example
gives the address of the **(0,0)th space in the well.**

```forth
0 th-w h. well h.
ca00 ca00 ok
$0405 th-w h. #45 well + h.
ca2d ca2d ok
$0405 th-c h. -4 40* 5 + colormem + h.
dae2 dae2 ok
0 th-q h. 3 th-q h. ( queue head/tail )
caee caed ok
```

[loc]: https://tetris.wiki/Glossary#L

### Data Shorthands `c: p:`

> [!TIP]
> You might want to refer to [the source][src] in a
> separate tab then Ctrl-F `: n:` to jump to:

```forth
: n: ( *'-*) parse-name evaluate ;
: c: ( u'-) 0 do n: c, loop ;
create colors 7 c: 3 8 6 4 5 2 7
```

`n: (*'-*)` asks the interpreter to parse `'` and interpret a
word. Since it could do literally anything I notate its stack
effect with `*`s but it's intended to parse number literals
for compiling data.

`c: (u'-)` loops `u` times, calling `n:` to parse a value and
`c,` to compile a character (i.e. byte) to memory.

`colors (-a)`, a `create`d word, pushes the `a`ddress of a
table of 7 color code bytes in the TTC color scheme. Without
the shorthand I could have just written this as:
`create colors 3 c, 8 c, 6 c, 4 c, 5 c, 2 c, 7 c,`

```forth
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p: ( '-) hex 8 0 do n: >p , loop decimal ;
```

`>p (c-p)` does precomputation: expanding an 8-bit `c`haracter
hex `$yx` into 16-bit `$0y0x`, then subtracting source center
`02` into compiled blockspace `p`osition `$0000`, which may or
may not contain a block. Negative x borrows from the y coord
but `hit?` bounds checks before you can corrupt memory outside
well and screenspace.

`p: ('-)` loops 8 times, parsing, expanding, and compiling hex
literals with `n: >p ,`.

### The `blocks` Table

```forth
create blocks \ center (c/.) at yx=02:
p: 00 01 02 03  02 12 22 32  \ iici
p: 00 01 02 03  02 12 22 32
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \     .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
( 4 shapes omitted. )
```

`blocks (-a)` gives the `a`ddress of the table. Again I could
have written this without shorthand as below but the goal was
for the data in the source to be compact and easier to read.

```forth
create blocks \ compiled blockspace coords:
-2 , -1 , 0 , 1 , 0 , $100 , $200 , $300 ,
( etc etc )
```

The ASCII-art comments depict the first orientation of each
shape, [TGM-style pointy-end-down][ars] (please click, there
are illustrations!), though they all rest on row 0 and all are
centered on column 2 (instead of TGM's 1 or 2) to simplify
code.

[ars]: https://tetris.wiki/Arika_Rotation_System

```forth
\ \ w = zp temp, lsb/msb,x = zp stack.
\ \ p@ scan table, add center (p)osition.
\ : w! ( a-) [ lsb ldy,x w sty, msb ldy,x
\   w 1+ sty, inx, 0 ldy,# ] ;
\ : p@ ( p-pp) dup [ clc, w lda,(y) iny,
\   lsb 1+ dup adc,x sta,x w lda,(y) iny,
\   msb 1+ dup adc,x sta,x ] ;
: p@ ( pa-ppa) dup >r @ over + swap r> 2+ ;

\ 7 shapes 4 turns 4 blocks 2 bytes.
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + ( w! ) p@ p@ p@ p@ 2drop r>
  colors + c@ ;
```

For speed sake the table scan words `w! (a-) p@ (p-pp)` are
written in assembly but for pedagogy sake I present above an
older combined Forth definition of `p@ (pa-ppa)`.

`p@ (pa-ppa)` takes a piece center position `p1` `$yyxx`, an
address in the blocks table `a1`, and fetches `@` and adds `+`
one cell of the table giving computed block position `p2`,
keeping the piece center `p3=p1`, and moving to the next table
address `a2=a1+2` ready to fetch the next block.

The Forth idiom `>r phrase r>` saves a value to the return
stack, allowing you to apply a `phrase` to the values
underneath it. Another word, `>10+>`, is named to evoke this
idiom, though it uses `swap`s for speed.

`piece (pts-ppppc)` takes a center `p`osition `$yyxx`, `t`urn
count `0-3`, and `s`hape index `0-6`, combines `t` and `s` to
index into the blocks table, calls `p@` to scan out 4
`p`ositions, and then a `c`olor.

Besides the assembly `w! p@`, the rest of the program is Forth
and just fast enough for [mostly full 50fps][per] during play.

[per]: #performance-and-tradeoffs

## Touring the Rest, Part 1: Game Stuff

```forth
: kbinit ( -) $b80 $28a ! 0 $c6 c! ;
: land ( -- gameover? ) kbinit ( ... ) ;
```

`kbinit` stores 3 bytes:

1. `$80` configures the KERNAL to repeat all keys,
   not just the cursors.
2. `$b` repeat delay of 11 frames.
3. `0` flushes the key buffer.

`land` calls it _first,_ before all its other work, to prevent
a keypress from leaking into the next piece, unless the player
continues to hold it for the 11 frames. 2+3 and 1 should
probably be separate words but I like the density.

```forth
: entropy ( -u) $a1 @ dup 0= + ;
: init ( u-) well size erase  seed !
  4 enqueue 5 enqueue 4 enqueue 4 roll
  enqueue  5 held!  enter  99 sig c! ;
: init ( u-) init qnext qnext qnext ;
: r ( -) 99 sig c@ <> if ;then kbinit
  bg #all d! begin draw step until ;
: new ( -) entropy init r ;
```

Starting a `new` game fetches part of the [jiffy counter][jif]
to seed the game state then enters the main loop, which is
named `r` for easy typing by the player.

The `dup 0= +` phrase in `entropy` ensures nonzero seed, which
was important for [an old xorshift PRNG][xor] and harmless 
with the current LCG. It's cute and I've grown fond of it.

[xor]: https://github.com/impomatic/xorshift798

The initial queue mimics [TGM randomizer][ran] behavior. First
the queue is [S, Z, S, random I/J/L/T], then after flushing
(`qnext` three times), the player starts with I/J/L/T, and the
next 3 pieces are less likely to be S or Z (4 or 5).

IJLT are first in the `blocks` table so that `4 roll enqueue`
can be simple.

[jif]: https://www.c64-wiki.com/wiki/160-162
[ran]: https://tetris.wiki/TGM_randomizer

```forth
\ roll (u-u) 0 <= u2 < u1.
: q? ( si-s/si-) th-q c@ over =
  if drop rdrop then ;
: qn ( -) 7 roll 0 q? 1 q? 2 q? 3 q?
  enqueue r> rdrop >r ;  12 profile
: qnext ( -) qn qn qn 7 roll enqueue ;
```

`qnext` itself reflects the complexity of the TGM algorithm.
It uses `rdrop` to affect control: either breaking out or
rolling again, though `qn` has to take care to dodge the
profiling instrument. Here's a more conventional version more
programmers will understand easier:

```forth
: reroll ( s-s) drop 7 roll ;
: q? ( sfi-sf) swap if drop 1 ;then
  th-q c@ over = ;
: qn ( sf-sf) if reroll 0
  0 q? 1 q? 2 q? 3 q? ;then 0 ;
: qnext ( -) 0 1 qn qn qn
  if reroll then enqueue ;
```

This isn't critical path so the codesize and cycle savings
aren't important but I choose to spend the extra `rdrop`
cognitive load just for its aesthetic, which I'm fond of.
`qnext` in both versions enqueues only once per call.

```forth
: t@+ ( t-t) turns c@ + 3 and ;
: curr+ ( pt-pts) swap pos @ +
  swap t@+  shape c@ ;
: curr+! ( pt-) t@+ turns c!  pos +! ;
: go ( pt-f) 2dup curr+ piece hit?
  if 2drop 1 ;then  curr+! #go d! 0 ;
$-100 constant down
: tk ( pt-) go 0= if rdrop rdrop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  rdrop ;
```

`go` adds `p`osition and `t`urn offsets to the current player
variables `pos` and `turns`, hit-checks the new hypothetical
piece position, and updates those variables if the piece can
move there. `turnkick` calls it up to six times to implement
[wallkicking][wal] upon rotation.

There are no [floor kicks][flo]. TGM3 allows limited I and T
floorkicking, but instead I have the horizontal rotations of
the I piece rest on row 0 so the player has less need, though
it's strictly easier than TGM in that sense.

[wal]: https://tetris.wiki/Wall_kick
[flo]: https://tetris.wiki/Floor_kick

## Touring the Rest, Part 2: Dev Stuff

<a href="https://imgur.com/xF5L3kF.png">
  <img alt="Example profile across 20+ frames." align="right"
  src="https://imgur.com/xF5L3kF.png" width="17%"/>
</a>

`redo` is a deeply magical development convenience:

```forth
marker --sss--
: redo ( -) --sss-- s" sss.fs"
  included ( must tco! safe w/ df. ) ;
```

Consider what happens when you type `redo` at the interpreter.
It executes the marker `--sss--`, deleting the program,
including `redo` itself. The processor doesn't care, it
continues executing the code, now in free memory, pushing the
`( addr len )` of the file name and then calling `included`,
which recompiles the source.

In most Forths, execution would then try to return from
`included` back to `redo` code that might have moved, crashing
the system in a fireball. DurexForth, however, optimizes the
tail-call into a jump, so `included` returns directly to the
interpreter safely.

You can then **resume a game in progress with the new code**
since the variables are outside the dictionary at `$cc00`,
chosen to overlap unused hi-res graphics, just after `v`'s
buffer.

```forth
create bx  $d020 eor, $d020 sta, rts,
: profile ( color -- ) here >r  dup
  lda,# bx jsr, latest >xt jsr, lda,#
  bx jmp,  r> latest name>string + ! ;
: '' ( "name" -- xt ) ' 6 + @ ;
: prof ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
```

The code at `bx` ("border xor") toggles the
[C64 border color register][bor] at `$d020`.
`profile` adjusts the latest word to point to new code:
`lda #color  jsr bx  jsr oldcode  lda #color  jmp bx`,
instrumenting the word with border-flipping behavior for [perf
measurement][per]. The phrase `name>string +` addresses the
code field stored after the name.

[bor]: https://www.c64-wiki.com/wiki/53280

`''` ticks through an instrumented word, recovering the
`oldcode` xt from the `jsr` instruction. For `dump`ing or
`execute`ing or whatever. Example: `'' sync 1+ c@ .` prints
the `215` operand from `sync`'s `lda,#` instruction below.

`0 prof` patches the first instruction at `bx` to an `rts`,
disabling it. `1 prof` restores the `eor`.

```forth
: sync ( -) [ 215 lda,# $d012 cmp,
  -5 bne, ] ;  6 profile
```

The `6 profile` here temporarily **undoes** the `6 profile` of
`draw`, returning to black border while waiting for sync.
Raster line 215 is near the bottom of the well so most `draw`
updates happen right after the scanline passes. Tradeoffs:

1. Hard code as above: correct by construction.
2. Parameterize on 8-bit input:
   incorrect for lines 0-54 and 256-311.
3. Parameterize on 9-bit input: more argument and loop code.
4. Actually learn raster interrupts: I don't wanna.

```forth
: bg ( ... ) $a0 ( rvspace ) ( ... ) ;
```

The reverse-video spaces make pleasant squares and also are
ignored by the interpreter to make testing and experimenting
easier.

## Performance and Tradeoffs

The PAL C64 has a ~19,700 cycle budget per 50Hz frame. Cycle
costs, eyeballing `1 prof` color bands:

| Frame% | While a Piece is in Play |
|--------|--------------------------|
| 0-7%   | KERNAL interrupt. Rolls through the frame, stepping on `sync` and dropping 1 frame every 4 or 5. |
| 13%    | idle `draw step`, tons of margin. |
| 17-19% | `piece hit?` move/gameover check. |
| 95%    | hold `j` to rotate every frame, very tight. |

Rotation wallkicks check up to 5 extra moves so might eat a
second, and maybe third, frame.

| Frame%   | After Landing a Piece |
|----------|-----------------------|
| 150-250% | `land`, depending on how much of the well `mark` scans and how many times `qnext` re-rolls. Then one of: |
| 105%     | `#next d! draw` if no lines, or: |
| 140%     | `#well d! draw` to show marked lines, then 11 idle `%stop` frames, then: |
| 160%     | `sweep` to erase marked lines and: |
| 260%     | `#all d! draw` to redraw most of the screen. |

Table 2 is less confident, +/- maybe 10%. I might be able to
spread work across frames to reduce well and queue flicker but
the complexity isn't worth it.

### Sound

I haven't learned any SID sound programming yet, and I fear
the extra code might strain the already cramped margins. I
also enjoy the aesthetic of very little 6502 code. Maybe I'm
worrying too much.

### Score

`lines` count progresses through the gravity frames table but
no scoring beyond that. The performance cost of computing
digits, the complexity cost of BCD, the digits on-screen
interfering with interpreter experimentation, just thinking
about it doesn't spark joy in me.

### Ghost Piece

I lament the missing [ghost piece][gho] and the
[instant drop][dro] that it enables. It's not possible to fit
in the frame budget as currently written. Not enough cycles to
check up to twenty `piece hit?`s.

[gho]: https://tetris.wiki/Ghost_piece
[dro]: https://tetris.wiki/Drop

Some approaches: caching, precalcing, incremental, probably
more. Some rough mental math and I think it might cost 30+
frames to compute all ghosts at entry time. I've seen NES
Tetrises with the feature but the complexity cost easily
outspends my joy budget.

<!-- end of DESIGN.md -->

