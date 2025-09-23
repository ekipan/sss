\ \ tetrablocks, for durexforth 4. \ \
marker --tet-- decimal
: redo ( -) --tet-- s" tet" included ;

-1 value -1  10 value #10     .( math )
: 40- #40 - ; : >10+> swap #10 + swap ;
: 4*  2* 2* ; : 10*  2* dup 4* + ;
: 0* drop 0 ; : 40*  4* 10* ;

: h. ( u-) hex u. decimal ;  .( tools )
create bx  $d020 eor, $d020 sta, rts,
: pro ( enable-time-profiling? -- )
  if $4d else $60 then bx c! ;
: profile ( color -- ) latest >xt
  here latest name>string + !  over
  lda,# bx jsr, jsr, lda,# bx jmp, ;
\ : pro drop ; : profile drop ;

13 22 ( col row ) 40* + dup     .( io )
$0400 + constant tilemem
$d800 + constant colormem
: theme ( fg $bgbd ) $d020 ! $286 c! ;
: sync ( -) [ 213 lda,# $d012 cmp,
  -5 bne, ] ;  6 profile
: kinit ( -) $b80 $28a ! 0 $c6 c! ;
: kpoll ( -c; w/ fast repeat hack.)
  key? if 1 $28b c! key else 0 then ;
: entropy ( -u) $a1 @ dup 0= + ;

: w! ( a) [ lsb ldy,x w sty, .( optim )
  msb ldy,x w 1+ sty, inx, 0 ldy,# ] ;
: p@ ( p-pp)  dup [ clc, w lda,(y) iny,
  lsb 1+ dup adc,x sta,x w lda,(y) iny,
  msb 1+ dup adc,x sta,x ] ;
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;

: else: ( -) postpone exit  .( syntax )
  postpone then ; immediate
: if\ ( f-) if postpone \ then ;
: field ( au-a) over constant + ;
: erase ( au-) 0 fill ;
: n: ( -?) parse-name evaluate ;
: c: ( u-) 0 do n: c, loop ;
: >p ( c-p) dup 4* 4* or $f0f and 2 - ;
: p:  hex 8 0 do n: >p , loop decimal ;

.( piece ) cr

create colors 7 c: 3 8 6 4 5 2 7
create blocks \ center (x/.) at yx=02:
p: 00 01 02 03  02 12 22 32  \ iixi
p: 00 01 02 03  02 12 22 32
p:  03 11 12 13  01 02 12 22 \   jjj
p:  01 02 03 11  02 12 22 23 \    .j
p: 01 11 12 13  02 12 22 21  \ lll
p: 01 02 03 13  03 02 12 22  \ l.
p:  02 11 12 13  02 11 12 22 \   ttt
p:  01 02 03 12  02 12 13 22 \    x
p: 01 02 12 13  02 11 12 21  \  ss
p: 01 02 12 13  02 11 12 21  \ sx
p:  03 02 11 12  02 12 13 23 \   zz
p:  03 02 11 12  02 12 13 23 \    xz
p: 01 02 11 12  01 02 11 12  \ oo
p: 01 02 11 12  01 02 11 12  \ ox

\ given piece (p)osition $yyxx, (t)urn
\ count 0-3, (s)hape 0-6, get 4 blocks
\ and a (c)olor. blocks $yx above were
\ precompiled into $0y0x minus center
\ $02. see p: compile p@ fetch/add.
: piece ( pts-ppppc) dup >r 4* + 4* 2*
  blocks + w! p@ p@ p@ p@ drop r>
  colors + c@ ;  14 profile

.( core )

here 249  2dup 2 fill  allot
210 field well \ 10 cols 22 rows of:
10 field spill \  0 empty, 1 marked,
0 field top    \  2-8 block colors.
2 field dirty \ bitset to draw.
2 field %stop \ n->0 line sweep timer.
2 field %grav \ n->0 fall timer.
2 field lines \ for gravity curve.
2 field seed  \ for random generator.
2 field qp    \ mod8 queue index:
8 field queue \ random shapes 0-6.
2 field p1    \ $yyxx from bottom left.
1 field t1    \ 0-3 clockwise turns.
1 field s1    \ 0-6 shape ijltszo.
1 field kept  \ 0-6 hold, w/ pin bit.
2 field p0    \ drawn piece to erase,
2 field t0    \ 'touch' to remember.
' well - ?dup 0=  if\ rvs . cr abort

\ fall frames, faster every 16 lines.
create gravs 6 c: 33 25 21 17 15 13
  10 c: 12 10 8 7 6 5 4 3 3 2

: th-c ( p-a) split 40* - colormem + ;
: th-w ( p-a) split 10* + well + ;
: th-y ( y-a) 10* well + ;
: th-q ( i-a) qp c@ + 7 and queue + ;
: th-g ( u-a) 4 rshift 15 min gravs + ;

: drawn ( -pts) p0 @ t0 @ split ;
: curr  ( -pts) p1 @ t1 @ split ;
: curr-y ( -y) p1 1+ c@ ;
: t1@+ ( t-t) t1 c@ + 3 and ;
: curr+ ( pt-pts)
  swap p1 @ +  swap t1@+  s1 c@ ;
: curr+! ( pt-) t1@+ t1 c!  p1 +! ;
: enter ( -) $1305 p1 ! 0 t1 c! ;
: touch ( -) p1 @ p0 ! t1 @ t0 ! ;

: pinned? ( -f) kept c@ 8 and ;
: kept@ ( -s) kept c@ 7 and ;
: kept! ( s-) kept c! ;
: unpin ( -) kept@ kept! ;
: keep ( -) kept@  s1 c@  8 or kept!
  s1 c! ;
: d? ( d-f) dirty @ and ;
: d! ( d-) dirty @ or dirty ! ;

: roll ( u-u; 0 <= u2 < u1.) seed @
  $7abd * $1b0f + dup seed !  um* nip ;
: enqueue ( s-) 1 qp +!  3 th-q c!
  0 th-q c@  s1 c! ;
here 5 c: -1 -1 4 4 5 \ discourage s/z.
: init ( u-) well ['] well well - erase
  seed ! 2 roll 4 + kept!  literal qp 5
  move  4 roll enqueue  enter touch ;

.( draw )

\ 21*19 backgrd of reverse spaces $a0.
\ +38 = +40 next line -2 left wall.
\ 19w = 2wall 10well 1wall 4next 2wall.
: bg ( -) tilemem 38 + 21 0 do  dup 19
  $a0 fill 40- loop  2+ #10 $a0 fill ;

\ set dirty bits d! to request redraw:
1 constant &curr   2 constant &next
4 constant &well   8 constant &kept
: &etc ( d-d) dup 1- or ;

\ store color codes into fg color mem.
: w+ ( aa-aa) 2dup #10 move >10+> 40- ;
  5 profile
: p! ( pc-c) dup rot th-c c! ;
: plot ( ppppc-) p! p! p! p! drop ;
: slot ( sp-) dup th-c 2 - dup 40-
  4 erase 4 erase 0 rot piece plot ;
: draw ( -) &curr d?  sync  if
    drawn piece 0* plot
  then  &well d? if
    well colormem begin
    w+ over spill = until  2drop
  then  &curr d? if
    curr piece plot  touch
  then  &next d? if
    1 th-q c@ $100d slot
    2 th-q c@ $0d0d slot
    3 th-q c@ $0a0d slot
  then  &kept d? if
    kept@ $050d slot
  then  0 dirty ! ;  6 profile

( rules )

( tgmlike, 4 rerolls. )       .( next )
: reroll ( s-s) drop 7 roll ;
: qdup? ( sfi-sf) swap if drop 1 else:
  th-q c@ over = ;  5 profile
: qroll ( sf-sf) if reroll 0 0 qdup?
  1 qdup? 2 qdup? 3 qdup? else: 0 ;
: qnext ( -) 0 1 qroll qroll qroll
  if reroll then enqueue ;  12 profile
: qflush ( -) qnext qnext qnext ;

( count/whiten/del rows. )    .( well )
: full? ( a-f) dup >10+> begin  dup c@
  while  1+ 2dup = until then  = ;
: m+ ( au-au) over full? if  1+ over
  #10 1 fill then  >10+> ;  7 profile
: mark ( y-u) th-y 0 m+ m+ m+ m+ nip ;
: s+ ( aa-aa) over c@ 1- if  2dup #10
  move #10 + then  >10+> ;  7 profile
: sweep ( -) well well begin  s+ over
  top = until  nip top over - erase ;

\ check, store into well.
: h? ( pf-f) swap dup  split #22 u<
  swap #10 u< and  if th-w c@ then or ;
: hit? ( ppppc-f) 0* h? h? h? h? ;
: l! ( pc-c) dup rot th-w c! ;
: lock ( ppppc-) l! l! l! l! drop ;

\ shift, turn. bias kicks ccw>l cw>r.
$-100 constant down         .( player )
: go? ( pt-f) 2dup curr+ piece hit?
  if 2drop 0 else: curr+! &curr d! 1 ;
: tk ( pt-) go? if r> r> 2drop then ;
: turnkick ( t-) >r  0 r@ tk  r@ r@ tk
  0 r@ - r@ tk  down r@ tk  down r@ +
  r@ tk  down r@ - r@ tk  r> drop ;

\ (g)ameover if entry blocked.
: trykeep ( -) pinned? if else:
  keep enter  &kept &curr or d! ;
: fall ( -g) down 0 go? if 0  else
  kinit unpin  curr piece lock
  curr-y mark ?dup if
    lines +!  12 %stop !  &well else
    &next &etc then d!  qnext
  enter touch  curr piece hit? then
  lines @ th-g c@ %grav ! ;

.( main )

: help  cr ." - game paused -"
cr ." enter [new] or [r]esume to play."
cr ." [sdf] move [jk] rotate [l] hold."
cr ." any other key to pause. " ;

: tick? ( a-f) -1 over +! c@ 0= ;
: step ( -g) %stop c@ if  %stop tick?
    if sweep &well &etc d! then  0
  else:  %grav tick? if fall else:
  kpoll case  0 of endof
  's' of -1 0 go? drop endof
  'd' of fall exit endof
  'f' of 1 0 go? drop endof
  'j' of -1 turnkick endof
  'k' of 1 turnkick endof
  'l' of trykeep endof
  page help exit endcase 0 ; 11 profile

: pre  kinit 11 0 theme page bg -1 d! ;
: new  0 pro entropy init qflush ;
: r  curr piece hit? if new else:
  pre begin draw step until ;
32 ' r 3 - c! \ patch new to fallthru.
' help start !  cr help

\ : nt parse-name find-name ; nt redo
\ nt pre latest - tuck - latest ( uaa)
\ swap rot over to latest move
