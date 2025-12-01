
[tet.fs][fs] is extremely dense, as its intended audience is
just myself. This is less a README and more a guided tour
hopefully more accessible to less longbearded folk. Code
excerpts might drift out-of-date. Fair warning.

---

[df]: https://github.com/jkotlinski/durexforth
[fs]: #end-of-readme

The [durexForth][df] system is available as either a C64
cartridge or disk image. After loading durexForth you can
either paste [the program][fs] directly, or get it on a disk
file somehow and `include tet.fs`. durexForth comes with a vi
clone if you want to make a file that way: `v tet.fs`. After
compiling try `1 prof 123 init r` and framestep to see
gamestate update time in gray and draw time in blue.

I use VICE's host filesystem feature to develop. It's not
officially supported by durexForth so it has a giant pile of
caveats but it's convenient enough that I use it anyway.
Caveats include needing ALL CAPS and CR line endings.

---

The heart of this program is the `piece` word that computes
block positions from a piece description. Here I ask for a
piece centered on row 9 column 5, rotated twice, shaped like a
J (shape index 1):

```forth
hex 905 2 1 piece .s
904 905 906 a04 8 ok
```

Coordinates hex `$yyxx` exist in three spaces:

- **Wellspace:** `0 <= y <= 22, 0 <= x <= 9`
- **Screenspace:** `0 <= y <= 20, 0 <= x <= 15.`
- **Blockspace:** `0 <= y <= 3, -2 <= x <= 1.`

Orange color `8`s can be [`lock`ed][dro] into wellspace if not
`hit?`-detected. Relative to bottom left, including 2 rows
above screen that newly-entered pieces can rotate into.

The `8`s are also `plot`ted on screen. Also relative bottom
left, physical row 22 column 13 so the whole canvas is roughly
screen-centered, with hold and next pieces off to the right
`12 <= x <= 15`.

Blockspace is relative to piece center, see `>p` below.

---

We'll start by defining data compilation shorthands:

```forth
: n: ( *'-*) parse-name evaluate ;
: c: ( u'-) 0 do n: c, loop ;
create colors 7 c: 3 8 6 4 5 2 7
```

`n: (*'-*)` asks the interpreter to parse `'` and interpret a
word. Since it could do literally anything I notate its effect
with `*`s but it's intended to parse number literals for
compiling data.

`c: (u'-)` loops `u` times, calling `n:` to parse a value and
`c,` to compile a character (i.e. byte) to memory.

`colors (-a)`, a `create`d word, pushes the `a`ddress after
itself, where I've compiled 7 bytes. C64 color codes for 7
Tetris pieces. Without the shorthand I could have just written
this as: `create colors 3 c, 8 c, 6 c, 4 c, 5 c, 2 c, 7 c,`

```forth
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p: ( '-) hex 8 0 do n: >p , loop decimal ;
```

`>p (c-p)` does precomputation: expanding an 8-bit `c`haracter
hex `$yx` into 16-bit `$0y0x`, then subtracting center source
`02` into compiled blockspace `p`osition `$0000`, which may or
may not contain a block. Negative x borrows from the y coord
but `hit?` bounds checks before you can corrupt memory outside
of well and screenspace.

`p: ('-)` loops 8 times, parsing hex literals `n:`, expanding
them `>p`, then compiling them `,`.

```forth
create blocks \ center (c/.) at yx=02:
p: 00 01 02 03  02 12 22 32  \ iici
p: 00 01 02 03  02 12 22 32
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \     .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
( 4 shapes omitted. )
: p@ ( pa-ppa) dup >r @ over + swap r> 2+ ;
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + p@ p@ p@ p@ 2drop r>
  colors + c@ ;
```

`blocks (-a)` gives the `a`ddress of the table. Again I could
have written this without shorthand `create blocks -2 , -1 , 0
, 1 , 0 , $100 , $200 , $300 , ( etc etc )` but the goal was
for the data in the source to be compact and easier to read.

For speed sake the table scanning word pair `w! (a-) p@ (p-pp)`
are written in assembly but for pedagogy sake I present here an
older combined Forth definition of `p@ (pa-ppa)`.

`p@ (pa-ppa)` takes a piece center position `p1` `$yyxx`, an
address in the blocks table `a1`, and fetches `@` and adds `+`
one cell of the table giving computed block position `p2`,
keeping the piece center `p3=p1`, and moving to the next table
address `a2=a1+2` ready to fetch the next block. The common
idiom `>r phrase r>` saves a value to the return stack,
allowing you to apply a `phrase` to the values underneath it.

`piece (pts-ppppc)` takes a center `p`osition `$yyxx`, `t`urn
count `0-3`, and `s`hape index `0-6`, combines `t` and `s` to
index into the blocks table, calls `p@` to scan out 4
`p`ositions, and then a `c`olor.

Besides the assembly `w! p@`, the rest of the program is Forth
and is just fast enough for mostly full 50fps during normal
play (Kernal interrupts eat every fifth-ish frame and landing a
piece takes like 3 or 4 but I'm okay with that).

---

A few other interesting parts:

```forth
: >10+> swap #10 + swap ;
```

The arrows are meant to evoke `>r 10 + r>`, but `swap` is
faster.

`redo` is a deeply magical development convenience:

```forth
marker --tet--
: redo ( -) --tet-- s" tet.fs"
  included ( must tco! safe w/ df. ) ;
```

Consider what happens when you type `redo` at the interpreter.
It executes the marker `--tet--`, deleting the program,
including `redo` itself. The processor doesn't care, it
continues executing the code, now in free memory, pushing the
`( addr len )` of the file name and then calling `included`,
which recompiles the source.

In most Forths, execution would then try to return from
`included` back to `redo` code that doesn't exist any more and
the system would crash in a fireball. `redo` as written here
relies on the fact that durexForth is subroutine-threaded and
tail-call optimized, so the call to `included` is instead a
jump, and when it's done it returns to the interpreter. Since
the game variables are in high memory outside the dictionary
you can immediately resume a game in progress with the new
code.

```forth
create bx  $d020 eor, $d020 sta, rts,
: profile ( color -- ) here >r  dup
  lda,# bx jsr, latest >xt jsr, lda,#
  bx jmp,  r> latest name>string + ! ;
: prof ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
```

The code at `bx` touches the C64 border color hardware register
`$d020`. `profile` adjusts the latest word to point to new
code: `lda #color  jsr bx  jsr oldcode  lda #color  jmp bx`,
instrumenting the word with border-flipping behavior for visual
time inspection while framestepping. The phrase `name>string +`
addresses the code field stored after the name.

`prof` patches the first instruction at `bx` into either an
`eor` or `rts`, enabling or disabling it.

```forth
: sync ( -) [ 213 lda,# $d012 cmp, \ hw
  -5 bne, ] ;  6 profile
```

Tradeoffs: (1) as above, hard code raster line 213: correct by
construction. (2) parameterize on 8-bit input: incorrect for
lines 0-54 and 256-311. (3) parameterize on 9-bit input: more
argument and loop code.

```forth
: bg ( ... ) $a0 ( rvspace ) ( ... ) ;
```

The reverse-video spaces make pleasant squares and also are
ignored by the interpreter to make testing and experimenting
easier.

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

Starting a `new` game fetches a Kernal frame counter to seed
the game state then enters the main loop, which is named `r`
for easy typing by the player.

The initial queue mimics [TGM randomizer][ran] behavior. First
the queue is [S, Z, S, random I/J/L/T], then after flushing
(`qnext` three times), the player starts with I/J/L/T, and the
next 3 pieces are less likely to be S or Z (4 or 5).

Most Tetris documentation lists the shapes alphabetically:
IJLOSTZ, I put IJLT first to simplify this init.

[ran]: https://tetris.wiki/TGM_randomizer

```forth
\ roll (u-u) 0 <= u2 < u1.
: q? ( si-s/si-) th-q c@ over =
  if drop rdrop then ;
: qn ( -) 7 roll 0 q? 1 q? 2 q? 3 q?
  enqueue r> rdrop >r ;  12 profile
: qnext ( -) qn qn qn 7 roll enqueue ;
```

`qnext` itself reflects the complexity of the TGM algorithm. It
uses `rdrop` to affect control: either breaking out or rolling
again, though `qn` has to take care to dodge the profiling
instrument. Here's a more conventional version more programmers
will understand easier:

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

---

One missing feature I lament is the [ghost piece][gho] (and the
[instant drop][dro] that it enables). I doubt it's possible to
fit in the frame budget without a large rearchitecting or
rewrite in mostly assembly. Just not enough cycles to calculate
so many `piece`s.

[gho]: https://tetris.wiki/Ghost_piece
[dro]: https://tetris.wiki/Drop

One approach I've considered is precalculating all possible
ghosts of the current piece at entry time. Some rough mental
math and I think it might cost 10+ish frames plus huge code
complexity.

I've seen NES Tetrises with the feature but again it'd probably
need an entire assembly rewrite or some sophisticated
caching/precalcing algorithm I've no idea about.

###### End of README.
