\ \ tetris for durexforth 4.
marker --tet-- decimal
: redo ( -) --tet-- s" tet" included ;

4 value 4  10 value #10
: 40-  40 - ; : >10+> swap #10 + swap ;
: 4*  2* 2* ; : 10*  dup 4* + 2* ;
: 0* drop 0 ; : 40*  10* 4* ;
: erase ( au-) 0 fill ;
: field ( au'-a) over value + ;
: else:  postpone exit postpone then ;
  immediate
: rdrop  pla, pla, ; immediate
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;

.( prim ) \ devtools, hw, draw, optim.

: h. ( u-) hex u. decimal ;
create bx  $d020 eor, $d020 sta, rts,
: prof ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
: profile ( color -- ) here >r  dup
  lda,# bx jsr, latest >xt jsr, lda,#
  bx jmp,  r> latest name>string + ! ;
\  : prof drop ; : profile drop ;

: entropy ( -u) $a1 @ dup 0= + ;
: kinit ( -) $b80 $28a ! 0 $c6 c! ;
: kpoll ( -c; w/ fast repeat hack.)
  key? if 1 $28b c! key else: 0 ;
: sync ( -) [ 213 lda,# $d012 cmp,
  -5 bne, ] ;  6 profile

13 22 ( row col ) 40* + dup
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

\ w! save table addr for p@ scan+add
\ center (p)osition. see piece.
: w! ( a) [ lsb ldy,x w sty, msb ldy,x
  w 1+ sty, inx, 0 ldy,# ] ;
: p@ ( p-pp)  dup [ clc, w lda,(y) iny,
  lsb 1+ dup adc,x sta,x w lda,(y) iny,
  msb 1+ dup adc,x sta,x ] ;

.( data ) \ parse, compile.

: n: ( '-*) parse-name evaluate ;
: c: ( u'-) 0 do n: c, loop ;
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p:  hex 8 0 do n: >p , loop decimal ;

create gravs 6 c: 33 25 21 17 15 13
10 c: 12 10 8 7 6 5 4 3 3 2 \ frames.
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

\ from center (p)osition $yyxx, (t)urn
\ count 0-3, (s)hape 0-6, get 4 blocks
\ and a (c)olor 2-8. blocks $yx above
\ were precompiled into $0y0x minus
\ center $02. see p: compile p@ scan.
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + w! p@ p@ p@ p@ drop r>
  colors + c@ ;  14 profile

.( core ) \ index, fetch, store.

$ca00 \ global game variables:
210 field well \ 10 cols 22 rows of:
10 field spill \  0 empty, 1 marked,
0 field top    \  2-8 block colors.
2 field p1    \ $yyxx from btm left.
1 field t1    \ 0-3 clockwise turns.
1 field s1    \ 0-6 shape ijltszo.
1 field held  \ 0-6 with pin bit 8.
8 field queue \ 0-6*8 random shapes.
2 field qh    \ queue head index mod8.
2 field seed  \ for random generator.
2 field lines \ for gravity curve.
2 field %grav \ n->0 fall timer.
2 field %stop \ n->0 line sweep timer.
2 field dirty \ bitflags to draw.
2 field p0    \ drawn piece to erase.
2 field t0
well - constant size

: th-g ( u-a) 4 rshift 15 min gravs + ;
: th-q ( i-a) qh c@ + 7 and queue + ;
: th-w ( p-a) split 10* + well + ;
: row ( -a) p1 1+ c@ 10* well + ;

: drawn ( -pts) p0 @ t0 @ split ;
: curr  ( -pts) p1 @ t1 @ split ;
: t1@+ ( t-t) t1 c@ + 3 and ;
: curr+ ( pt-pts) swap p1 @ +
  swap t1@+  s1 c@ ;
: curr+! ( pt-) t1@+ t1 c!  p1 +! ;
: enter ( -) $1305 p1 ! 0 t1 c! ;

: pinned? ( -f) held c@ 8 and ;
: held@ ( -s) held c@ 7 and ;
: held! ( s-) held c! ;
: unpin ( -) held@ held! ;
: hold ( -) held@  s1 c@ 8 or held!
  s1 c! ;
: enqueue ( s-) 1 qh +!  3 th-q c!
  0 th-q c@ s1 c! ;

: roll ( u-u; 0 <= u2 < u1.) seed @
  $7abd * $1b0f + dup seed !  um* nip ;
: init ( u-) well size erase  seed !
  4 enqueue 5 enqueue 4 enqueue
  4 roll enqueue  4 held!  enter ;

.( draw ) \ with dirty bitset.

$01 constant #prev  $03 constant #go
$02 constant #curr  $06 constant #next
$04 constant #queue $0b constant #hold
$08 constant #held  $1e constant #all
$10 constant #well

: d? ( d-f) dirty @ and ;
: d! ( d-) dirty @ or dirty ! ;
: slot ( sp) dup rub 0 rot piece plot ;
: draw ( -) sync  #prev d? if
    drawn piece 0* plot
  then  #well d? if
    spill well paint
  then  #curr d? if
    curr piece plot  p1 p0 4 move
  then  #queue d? if
    1 th-q c@ $100d slot
    2 th-q c@ $0d0d slot
    3 th-q c@ $0a0d slot
  then  #held d? if
    held@ $050d slot
  then  0 dirty ! ;  6 profile

.( rules ) \ queue, well, player.

\ tgmlike, reroll dupes.
: reroll ( s-s) drop 7 roll ; 5 profile
: qdup? ( si-s) th-q c@ over =
  if rdrop then ;
: qtry ( s-s/s-) reroll 0 qdup? 1 qdup?
  2 qdup? 3 qdup? enqueue rdrop ;
: qnext ( -) 0 qtry qtry qtry reroll
  enqueue ;  12 profile
: init ( u-) init qnext qnext qnext ;

\ count, whiten, delete rows.
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

\ move. kick bias ccw>l cw>r. f hit?
$-100 constant down
: go ( pt-f) 2dup curr+ piece hit?
  if 2drop 1 else:  curr+! #go d! 0 ;
: tk ( pt-) go 0= if rdrop rdrop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  rdrop ;
: tally ( -) row mark ?dup if  lines +!
  12 %stop ! #well d! else:  #next d! ;
: fall ( -f) down 0 go 0= if  0 else
  kinit unpin  curr piece lock  tally
  qnext enter  curr piece hit?
  then  lines @ th-g c@ %grav ! ;
: tryhold ( -) pinned? if else:
  hold enter  #hold d! ;

.( main ) cr \ timers, input.

: help  ." - game paused -" cr
." enter [new] or [r]esume to play." cr
." [sdf] move [jk] rotate [l] hold." cr
." any other key to pause. " cr ;

: tick ( a-f) -1 over +! c@ 0= ;
: idle ( -) %stop tick if
  sweep #all d! then ;
: step ( -- gameover? )
  %stop c@ if idle 0 else:
  %grav tick if fall else:
  kpoll 0 of 0 else:
  'd' of fall else:
  's' of -1 0 go 0* else:
  'f' of 1 0 go 0* else:
  'j' of -1 turnkick 0 else:
  'k' of 1 turnkick 0 else:
  'l' of tryhold 0 else:
  page help ;  11 profile

: r  kinit bg #all d! curr piece hit?
  if draw else: begin draw step until ;
: new  0 prof entropy init r ;
' help start ! help

: dd  bg #all d! draw ;
: ss  well $c800 size move ;
: ll  $c800 well size move ;

\ a piece is described by a triple:
\   (p)osition $yyxx from bottom left.
\   (t)urns clockwise 0-3.
\   (s)hape 0-6 ijltszo.
\ stored into gamestate:
\   enqueue (s-) ->tail, head->player.
\   enter (-) top of well, unturned.
\   curr+! (pt-)
\   hold (-) swap s and held s.
\ taken from gamestate:
\   drawn (-pts)
\   curr (-pts)
\   curr+ (pt-pts)
\ computed into four blocks:
\   piece (pts-ppppc)
\ then hit detected or stored:
\   hit? (ppppc-f) in playfield
\   lock (ppppc-) into playfield
\   plot (ppppc-) onto screen
