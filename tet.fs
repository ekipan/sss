marker --tet-- \ tested w/ durexforth4.

\ 0.  groundwork.

: h. ( u-) hex u. decimal ; \ devtools.
: redo ( -) --tet-- s" tet" included ;
create bd/  $d020 eor, $d020 sta, rts,
: pro ( enable? -- ) if
  $4d else $60 then bd/ c! ;
: profile ( color -- ) latest >xt over
  here latest name>string + ! ( cxc)
  lda,# bd/ jsr, jsr, lda,# bd/ jmp, ;
\ : pro drop ; : profile drop ;

: else:  postpone exit        \ macros.
  postpone then ; immediate
: dup>r  postpone >r dex, ; immediate
: erase ( au-) 0 fill ;       \ basics.
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;
-1 value -1 3 value 3       \ fastmath.
10 value #10 22 value #22 40 value #40
: 40- #40 - ; : >10+> swap #10 + swap ;
: 4* 2* 2* ;  : 10* 2* dup 4* + ;
: 0* drop 0 ; : 40* 4* 10* ;

: sync ( -) [ $d5 lda,#     \ hardware.
  $d012 cmp, -5 bne, ] ;  13 profile
: hue ( fg $bgbd -- ) $d020 ! $286 c! ;
: entropy ( -u) $a1 @ 1 or ;
: kbflush ( -) $c80 $28a ! 0 $c6 c! ;
: kb ( -c; w/ hacky fast repeat.)
  key? if 1 $28b c! key else: 0 ;

: xsh ( u-u; xorshift-798.)    \ optim.
  [ msb lda,x ror,a lsb lda,x ror,a
  msb eor,x msb sta,x ror,a lsb eor,x
  lsb sta,x msb eor,x msb sta,x ] ;
: wy! ( nn-) [ lsb ldy,x inx, lsb lda,x
  w sta, msb lda,x w 1+ sta, inx, ] ;
: full? ( a-f) 1- #10 wy! [ w lda,(y)
  6 beq, dey, -7 bne, ] 1 exit 0 ;
: b@+ ( b-bb) dup [ clc, w lda,(y) iny,
  lsb 1+ dup adc,x sta,x w lda,(y) iny,
  msb 1+ dup adc,x sta,x ] ;

1 . \ piece definition.

: n: ( -?) parse-name evaluate ;
: c: ( u-) 0 do n: c, loop ;
: >b ( c-b) dup 4* 4* or $f0f and 2 - ;
: b:  hex 8 0 do n: >b , loop decimal ;

create colors 7 c: 3 8 6 4 5 2 7
create blocks \ compiled as $0y0x-2.
b: 00 01 02 03  02 12 22 32  \ iixi
b: 00 01 02 03  02 12 22 32  \
b:  03 11 12 13  01 02 12 22 \   jjj
b:  01 02 03 11  02 12 22 23 \    .j
b: 01 11 12 13  02 12 22 21  \ lll
b: 01 02 03 13  03 02 12 22  \ l.
b:  02 11 12 13  02 11 12 22 \   ttt
b:  01 02 03 12  02 12 13 22 \    x
b: 01 02 12 13  02 11 12 21  \  ss
b: 01 02 12 13  02 11 12 21  \ sx
b:  03 02 11 12  02 12 13 23 \   zz
b:  03 02 11 12  02 12 13 23 \    xz
b: 01 02 11 12  01 02 11 12  \ oo
b: 01 02 11 12  01 02 11 12  \ ox

\ given a (b)lock origin $yyxx, (t)urn
\ count 0-3, and (s)hape 0-6, fetch 4
\ (b)locks and a (c)olor.
: piece ( bts-bbbbc) dup>r 4* + 4* 2*
  blocks + 0 wy! b@+ b@+ b@+ b@+ drop
  r> colors + c@ ;  5 profile

2 . \ game state.

: if\ ( f-) if postpone \ then ;
: field ( au-a) over constant + ;

here 247 allot
210 field well  \ 10 cols 22 rows of:
10 field vistop \  0 empty, 1 marked,
0 field welltop \  2-8 block colors.
2 field %show \ n->0 line sweep timer.
2 field %grav \ n->0 fall timer.
2 field lines \ todo: gravity curve.
1 field kept  \ 0-6  hold, w/ pin bit.
2 field qp    \ mod8 queue index:
8 field queue \ random shapes 0-6.
2 field seed  \ cannot be zero!
2 field b1    \ $yyxx block position.
1 field t1    \ 0-3   clkwise turns.
1 field s1    \ 0-6   shape ijltszo.
2 field b0    \ $yyxx drawn previous
1 field t0    \ 0-3   frame, to be
1 field s0    \ 0-6   erased.
' well - ?dup 0= if\ rvs . cr abort

3 . \ tgmlike randomizer.

\ usually called a 'hold piece' but
\ i don't want to clash w/ 'hold'.
: kept! ( s-) kept c! ;
: kept@ ( -s) kept c@ 7 and ;
: pinned? ( -f) kept c@ 8 and ;
: unpin ( -) kept@ kept! ;
: keep ( -) kept@  s1 c@  8 or kept!
  s1 c! ;

: roll ( u-u) seed @ xsh dup seed !
  um* nip ;  5 profile
: reroll ( s-s) drop 7 roll ;

\ (f) reroll? if dupe, up to 4 times.
: th-q ( i-a) qp c@ + 7 and queue + ;
: q! ( s-) 3 th-q c! ;
: qdup? ( sfi-sf) swap if drop 1 else:
  th-q c@ over = ;  15 profile
: qtry ( sf-sf) if reroll 0 0 qdup?
  1 qdup? 2 qdup? 3 qdup? else: 0 ;
: qnext ( -) 0 1 qtry qtry qtry if
  reroll then  1 qp +! q!  0 th-q c@
  s1 c! ;  7 profile

\ open w/ ijlt, bias against sz.
create ssz 5 c: 0 0 4 4 5
: qflush ( -) 2 roll 4 + kept!  ssz qp
  5 move  4 roll q! qnext qnext qnext ;

4 . \ the well.

\ move unmarked lines down, erase top.
: drain ( a-) welltop over - erase ;
: 10move ( aa-) #10 move ;  12 profile
: sweep ( -) well well begin  over c@
  1- if 2dup 10move #10 + then >10+>
  over welltop = until  drain drop ;

\ whiten filled lines, give count (u).
: th-r ( y-a) 10* well + ;
: m+ ( au-au) over full? if  1+ over
  #10 1 fill then  >10+> ;  9 profile
: mark ( y-u) th-r 0 m+ m+ m+ m+ nip ;

\ check well, store into well.
\ h? reuses oob indices as true flag.
: th-b ( b-a) split 10* + well + ;
  4 profile
: h? ( bf-f) swap dup split #22 u<
  swap #10 u< and if th-b c@ then or ;
: hit? ( bbbbc-f) 0* h? h? h? h? 0<> ;
: l! ( bc-c) tuck swap th-b c! ;
: lock ( bbbbc-) l! l! l! l! drop ;

\ pre-5.  draw/update shared state.

1 constant &curr \ in(v)alidation
2 constant &next \ queue. set bits
4 constant &well \ to request
8 constant &kept \ redraw.
0 value invalid
: inv? ( v-f) invalid and ;
: inv! ( v-) invalid or to invalid ;
: &+ ( v-v) dup 1- or ;

\ the drawn piece is touched to erase
\ next frame. touch a new one to leave
\ the old one on screen.
: touch ( -) b1 b0 4 move ;
: drawn ( -bts) b0 @ t0 @ split ;
: curr  ( -bts) b1 @ t1 @ split ;
: curr-y ( -y) b1 1+ c@ ;

5 . \ player update.

\ move piece. kick into turn direction.
$-100 constant down
: t1@+ ( t-t) t1 c@ + 3 and ;
: go? ( fbt-f) rot 0= if 2drop 0 else:
  over b1 @ +  over t1@+  s1 c@
  piece hit? if 2drop 1 else:
  t1@+ t1 c!  b1 +!  &curr inv! 0 ;
: turnkick ( t-) dup>r 0 i go? i i go?
  0 i - i go? down i go? down i + i go?
  down i - r> go? drop ;

\ (g)ameover if entry blocked.
: enter ( -) $1305 b1 ! 0 t1 c! ;
: trykeep ( -) pinned? if else:
  keep enter  &kept &curr or inv! ;
: fall ( -g) 12 %grav !  1 down 0 go?
  0= if 0 else:  kbflush unpin
  curr piece lock  curr-y mark ?dup if
    lines +! 12 %show !
  &well else &next &+ then  inv!
  qnext enter touch  curr piece hit? ;
: init ( u-) well seed well - erase
  seed !  qflush enter touch ;

: tick? ( a-f) -1 over +! c@ 0= ;
: update ( -g) %show c@ if  %show tick?
    if sweep &well &+ inv! then 0
  else:  %grav tick? if fall else:
  kb case  0 of endof
  's' of 1 -1 0 go? drop endof
  'd' of fall exit endof
  'f' of 1 1 0 go? drop endof
  'j' of -1 turnkick endof
  'k' of 1 turnkick endof
  'l' of trykeep endof
  drop 1 exit endcase 0 ;

6 . \ drawing.

$077d value screen   \ bottom left
$db7d value colormem \ of well.

: th-cm ( b-a) split 40* - colormem + ;
: p! ( bc-c) tuck swap th-cm c! ;
: plot ( bbbbc-) p! p! p! p! drop ;
: slot ( sb-) dup th-cm 2 - dup 40-
  4 erase 4 erase 0 rot piece plot ;

: draw ( -) &curr inv?  sync  if
    drawn piece 0* plot
  then  &well inv? if
    well colormem begin
    2dup 10move >10+> 40-
    over vistop = until  2drop
  then  &curr inv? if
    curr piece plot  touch
  then  &kept inv? if
    kept@ $060d slot
  then  &next inv? if
    1 th-q c@ $100d slot
    2 th-q c@ $0d0d slot
    3 th-q c@ $0a0d slot
  then  0 to invalid ;  6 profile
: dr ( -) -1 inv! draw ;

\ 7.  main loop.

: help ( -) ." keys: sdf jkl q"
  ."       cmds: new r(esume)" cr ;
: prep ( -) kbflush 11 0 hue page help
  screen 38 + 21 0 do  dup 19 $a0 fill
  40- loop  2+ #10 $a0 fill dr ;
: r ( -) prep begin update draw until ;
: new ( -) 0 pro entropy init r ;
1234 init ' help start !
