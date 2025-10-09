\ \ tetris for durexforth 4.

marker --tet--  decimal
: h. ( u-) hex u. decimal ;
: erase ( au-) 0 fill ;
: ;then ( a-:-) postpone exit
  postpone then ; immediate
4 value 4     10 value #10
: 40-  40 - ; : >10+> swap #10 + swap ;
: 4*  2* 2* ; : 10*  dup 4* + 2* ;
: 0* drop 0 ; : 40*  10* 4* ;

6 . \ nonportable.

: redo ( -) --tet-- s" tet" included ;
create bx  $d020 eor, $d020 sta, rts,
: profile ( color -- ) here >r  dup
  lda,# bx jsr, latest >xt jsr, lda,#
  bx jmp,  r> latest name>string + ! ;
: prof ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
\ : prof drop ; : profile drop ;

\ w! p@ save table addr then    \ optim
\ scan+add center (p)os. see piece
: w! ( a-) [ lsb ldy,x w sty, msb ldy,x
  w 1+ sty, inx, 0 ldy,# ] ;
: p@ ( p-pp) dup [ clc, w lda,(y) iny,
  lsb 1+ dup adc,x sta,x w lda,(y) iny,
  msb 1+ dup adc,x sta,x ] ;
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;
: rdrop ( n~) pla, pla, ; immediate

: sync ( -) [ 213 lda,#            \ hw
  $d012 cmp, -5 bne, ] ;  6 profile
: kbinit ( -) $b80 $28a ! 0 $c6 c! ;
: kbpoll ( -c; w/ fast repeat hack.)
  key? if 1 $28b c! key ;then 0 ;
: entropy ( -u) $a1 @ dup 0= + ;

13 22 ( row col ) 40* + dup    \ screen
$d800 + constant colormem
$0400 + constant tilemem
: th-c ( p-a) split 40* - colormem + ;
: p! ( pc-c) dup rot th-c c! ;
: plot ( ppppc-) p! p! p! p! drop ;
: rub ( p-) th-c 2 - dup 4 erase
  40- 4 erase ;
: paint ( aa-) colormem begin
  2dup #10 move >10+> 40-
  over 3 pick = until  drop 2drop ;
: bg ( -; $a0 rvs spaces 21x19 grid.)
  11 $286 c! 0 $d020 ! page tilemem
  38 + 21 0 do  dup 19 $a0 fill
  40- loop  2+ #10 $a0 fill ;

5 . \ data, piece definition.

: n: ( '-*) parse-name evaluate ;
: c: ( u'-) 0 do n: c, loop ;
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p:  hex 8 0 do n: >p , loop decimal ;

create gravs 5 c: 33 25 21 17 15
11 c: 13 12 10 8 7 6 5 4 3 3 2
create colors 7 c: 3 8 6 4 5 2 7
create blocks \ center (c/.) at yx=02:
p: 00 01 02 03  02 12 22 32  \ iici
p: 00 01 02 03  02 12 22 32
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \     .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
p:  02 11 12 13  02 11 12 22 \    ttt
p:  01 02 03 12  02 12 13 22 \     c
p: 01 02 12 13  02 11 12 21  \  ss
p: 01 02 12 13  02 11 12 21  \ sc
p:  03 02 11 12  02 12 13 23 \    zz
p:  03 02 11 12  02 12 13 23 \     cz
p: 01 02 11 12  01 02 11 12  \ oo
p: 01 02 11 12  01 02 11 12  \ oc

: grav ( u-u) 15 min gravs + c@ ;
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + w! p@ p@ p@ p@ drop r>
  colors + c@ ;  14 profile

\ a piece is: center (p)osition hex
\ $yyxx from bottom left, clockwise
\ (t)urns count 0-3, and (s)hape index
\ 0-6 ijltszo. stored to gamestate:
\   enqueue (s-) ->tail, head->player.
\   enter (-) row 19 col 5 turns 0.
\   curr+! (pt-) move/rotate player.
\   hold (-) swap s and reserve s.
\   >old (-) remember drawn piece.
\ fetched back from gamestate:
\   old@ (-pts) to erase from screen.
\   curr (-pts) in play.
\   curr+ (pt-pts) to check a move.
\ then block positions computed:
\   piece (pts-ppppc) and (c)olor.
\   hit? (ppppc-f) check playfield.
\   lock (ppppc-) store to playfield.
\   plot (ppppc-) store to screen.

4 . \ core: vars, index, fetch, store.

: var+ ( au'-a) over value + ;
$ca00 \ global game variables:
210 var+ well \ 10 by 22 playfield of:
10 var+ spill \   0 empty, 1 marked,
0 var+ top    \   2-8 block colors.
2 var+ pos   \ $yyxx \
1 var+ turns \ 0-3    ) falling piece.
1 var+ shape \ 0-6   /
1 var+ held  \ 0-6 with pin bit $08.
8 var+ queue \ 0-6 next random shapes.
2 var+ qidx  \ mod8 queue head index.
2 var+ seed  \ for random generator.
2 var+ lines \ for gravity curve.
2 var+ %grav \ n->0 fall timer.
2 var+ %stop \ n->0 line sweep timer.
1 var+ sig   \ 99 if initialized.
well - constant size

: th-q ( i-a) qidx c@ + 7 and queue + ;
: th-w ( p-a) split 10* + well + ;
: row ( -a) pos 1+ c@ 10* well + ;

: curr ( -pts) pos @ turns @ split ;
: t@+ ( t-t) turns c@ + 3 and ;
: curr+ ( pt-pts) swap pos @ +
  swap t@+  shape c@ ;
: curr+! ( pt-) t@+ turns c!  pos +! ;
: enter ( -) $1305 pos ! 0 turns c! ;

: pinned? ( -f) held c@ 8 and ;
: held@ ( -s) held c@ 7 and ;
: held! ( s-) held c! ;
: unpin ( -) held@ held! ;
: hold ( -) held@  shape c@ 8 or held!
  shape c! ;
: enqueue ( s-) 1 qidx +!  3 th-q c!
  0 th-q c@ shape c! ;

: roll ( u-u; 0 <= u2 < u1.) seed @
  $7abd * $1b0f + dup seed !  um* nip ;
: init ( u-) well size erase  seed !
  4 enqueue 5 enqueue 4 enqueue 4 roll
  enqueue  5 held!  enter  99 sig c! ;

3 . \ draw, with dirty bitset.

variable old 0 , \ drawn, to erase.
: old@ ( -pts) old @ old 2+ @ split ;
: >old ( -) pos old 4 move ;

variable dirty \ bitflags to draw.
$01 constant #old   $02 constant #curr
$04 constant #queue $08 constant #held
$10 constant #well
: d? ( d-f) dirty @ and ;

\ 6: rub (p-) plot (ppppc-) paint (aa-)
: slot ( sp) dup rub 0 rot piece plot ;
: draw ( -) sync
  #well d? if spill well paint then
  #old d? if old@ piece 0* plot then
  #curr d? if curr piece plot >old then
  #queue d? if 1 th-q c@ $110d slot
    2 th-q c@ $0e0d slot
    3 th-q c@ $0b0d slot then
  #held d? if held@ $050d slot then
  0 dirty ! ;  6 profile

2 . \ rules: queue, well, player.

\ tgmlike reroller rng. 4: roll (u-u).
: q? ( si-s/si-) th-q c@ over =
  if drop rdrop then ;
: qn ( -) 7 roll 0 q? 1 q? 2 q? 3 q?
  enqueue rdrop ;
: qnext ( -) qn qn qn 7 roll enqueue ;
: init ( u-) init qnext qnext qnext ;

\ count, whiten, delete rows (a).
: full? ( a-f) dup >10+> begin  dup c@
  while  1+ 2dup = until then  = ;
: m+ ( au-au) over full? if  1+ over
  #10 1 fill then  >10+> ;  7 profile
: mark ( a-u) 0 m+ m+ m+ m+ nip ;
: s+ ( aa-aa) over c@ 1- if  2dup #10
  move #10 + then  >10+> ;  7 profile
: sweep ( -) well well begin  s+ over
  top = until  nip top over - erase ;

\ check, store into well.
: h? ( pf-f) swap dup  split 22 u<
  swap #10 u< and  if th-w c@ then or ;
: hit? ( ppppc-f) 0* h? h? h? h? ;
: l! ( pc-c) dup rot th-w c! ;
: lock ( ppppc-) l! l! l! l! drop ;

$03 constant #go    $06 constant #next
$0b constant #hold  $1e constant #all
: d! ( d-) dirty @ or dirty ! ;

\ move. kick bias ccw>l cw>r. f hit?
$-100 constant down
: go ( pt-f) 2dup curr+ piece hit?
  if 2drop 1 ;then  curr+! #go d! 0 ;
: tk ( pt-) go 0= if rdrop rdrop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  rdrop ;
: tally ( -) row mark ?dup if  lines +!
  12 %stop ! #well d! ;then  #next d! ;
: fall ( -f) down 0 go 0= if  0 else
  kbinit unpin  curr piece lock  tally
  qnext enter  curr piece hit? then
  lines @ 3 rshift grav %grav ! ;
: tryhold ( -) pinned? if ;then
  hold enter  #hold d! ;

1 . \ main: timers, input.

: help  cr ." - game paused -"
cr ." enter [new] or [r]esume to play."
cr ." [sdf] move [jk] rotate [l] hold."
cr ." any other key to pause. " cr ;

: tick ( a-f) -1 over +! c@ 0= ;
: step ( -- gameover? )
  %stop c@ if %stop tick if
    sweep #all d! then 0 ;then
  %grav tick if fall ;then
  kbpoll 0 of 0 ;then
  'd' of fall ;then
  's' of -1 0 go 0* ;then
  'f' of 1 0 go 0* ;then
  'j' of -1 turnkick 0 ;then
  'k' of 1 turnkick 0 ;then
  'l' of tryhold 0 ;then
  page help ;  11 profile

: r ( -) 99 sig c@ <> if ;then kbinit
  bg #all d! begin draw step until ;
: new ( -) entropy init r ;
0 prof ' help start !

: dd ( -) bg #all d! draw ;
: ss ( -) well $c800 size move ;
: ll ( -) $c800 well size move ;
