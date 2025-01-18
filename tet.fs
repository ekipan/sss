marker --tet-- \ in durexforth v4.

: .h ( u-) hex u. decimal ; \ devtools.
: make ( -) --tet-- s" tet" included ;
code p/  $d020 eor, $d020 sta, rts,
: pro  if $4d else $60 then ['] p/ c! ;
: profile ( c-) here swap dup lda,#
  ['] p/ jsr, latest >xt jsr, lda,#
  ['] p/ jmp, latest name>string + ! ;

: else:  postpone exit        \ macros.
  postpone then ; immediate
: dup>r  postpone >r dex, ; immediate

: erase ( au-) 0 fill ;       \ basics.
: bounds ( au-aa) over + swap ;
: split ( $yyxx -- $xx $yy ) [ 0 ldy,#
  msb lda,x msb sty,x ] pushya ;
: w! ( n-) [ lsb lda,x msb ldy,x inx,
  w sta, w 1+ sty, ] ;

-1 value -1 3 value 3           \ math.
10 value #10 23 value #23 40 value #40
: >10+> swap #10 + swap ;
: 0* drop 0 ;   : 10* 2* dup 2* 2* + ;
: 40- #40 - ;   : 40* 2* 2* 10* ;

: kbrep ( -) $80 $28a c! ;  \ keyboard.
: kbflush ( -) 16 $28b c! 0 $c6 c! ;
: kb  key? if 1 $28b c! key else: 0 ;

1 . \ piece definition.

: n: ( -?) parse-name evaluate ;
: c: ( u-) 0 do n: c, loop ;
: p: ( -) hex 8 0 do n: dup 4 lshift
  or $f0f and 2 - , loop decimal ;

create patterns \ compiled as $0y0x-2.
p: 00 01 02 03  02 12 22 32  \ oiii
p: 00 01 02 03  02 12 22 32  \
p:  03 11 12 13  01 02 12 22 \    jjj
p:  01 02 03 11  02 12 22 23 \   .  j
p: 01 11 12 13  02 12 22 21  \  lll
p: 01 02 03 13  03 02 12 22  \ .l
p:  02 11 12 13  02 11 12 22 \    ttt
p:  01 02 03 12  02 12 13 22 \   . t
p: 01 02 12 13  02 11 12 21  \   ss
p: 01 02 12 13  02 11 12 21  \ .ss
p:  03 02 11 12  02 12 13 23 \    zz
p:  03 02 11 12  02 12 13 23 \   . zz
p: 01 02 11 12  01 02 11 12  \  oo
p: 01 02 11 12  01 02 11 12  \ .oo

create ttc 7 c: 3 8 6 4 5 2 7
create tgm 7 c: 2 8 6 3 4 5 7
ttc value colors

\ compute 4 (b)lock coords $yyxx and a
\ (c)olor for a piece given by origin,
\ (t)urn count 0-3, and (s)hape 0-6.
: b@+ ( ba-bba) dup>r @ bounds r> 2+ ;
: piece ( bts-bbbbc) dup>r 2* 2* +
  2* 2* 2* patterns + b@+ b@+ b@+ b@+
  2drop r> colors + c@ ;  5 profile

2 . \ game state.

: if\ ( f-) if postpone \ then ;
: field ( au-a) over constant + ;

here 256 2dup erase allot
210 field well  \ 10 cols 23 rows of:
20 field vistop \  0 empty, 1 marked,
0 field welltop \  2-8 block colors.
2 field %show  \ n->0 line sweep timer.
2 field %grav  \ n->0 fall timer.
2 field lines  \ todo: gravity curve.
2 field seed   \ cannot be zero!
1 field kept   \ 0-6  hold, w/ pin bit.
2 field qp     \ mod8 queue index:
8 field queue  \ random shapes 0-6.
2 field b1     \ $yyxx block position.
1 field t1     \ 0-3   clkwise turns.
queue value s1 \ cached queue addr.
2 field b0     \ $yyxx drawn previous
1 field t0     \ 0-3   frame, to be
1 field s0     \ 0-6   erased.
' well - ?dup 0= if\ rvs . cr abort

3 . \ shapes queue.

\ usually called a 'hold piece' but
\ i don't want to clash w/ 'hold'.
: kept! ( s-) kept c! ;
: kept@ ( -s) kept c@ 7 and ;
: pinned? ( -f) kept c@ 8 and ;
: unpin ( -) kept@ kept! ;
: keep ( -) kept@  s1 c@ 8 or  kept!
  s1 c! ;

: roll ( u-u; xorshift-798.) [ seed 1+
  lda, ror,a seed lda, ror,a seed 1+
  dup eor, sta, ror,a seed dup eor,
  sta, seed 1+ dup eor, sta, tay, seed
  lda, ] pushya um* nip ;  5 profile
: reroll ( s-s) drop 7 roll ;

\ (f) reroll? if dupe, up to 4 times.
: th-q ( i-a) qp c@ + 7 and queue + ;
: qdup ( sfi-sf) swap if drop 1 else:
  th-q c@ over = ;  15 profile
: qtry ( sf-sf) if reroll 0 0 qdup
  1 qdup 2 qdup 3 qdup else: 0 ;
: q+ ( n-) qp +!  0 th-q to s1 ;
: q! ( s-) 3 th-q c! ;
: qnext ( -) 0 1 qtry qtry qtry
  if reroll then 1 q+ q! ;  7 profile

\ open w/ ijlt, bias against sz.
create ssz 5 c: 0 0 4 4 5
: qflush ( -) ssz qp 5 move  4 roll q!
  qnext qnext qnext  2 roll 4 + kept! ;

4 . \ the well.

\ move unmarked lines down, erase top.
: drain ( a-) welltop over - erase ;
: 10move ( aa-) #10 move ;  12 profile
: sweep ( -) well well begin  over c@
  1- if 2dup 10move #10 + then >10+>
  over welltop < 0= until  drain drop ;

\ whiten filled lines, give count (u).
: line ( au-au) over 1- w! [ 10 ldy,#
  w lda,(y) 1 bne, rts, dey, -8 bne, ]
  over #10 1 fill  1+ ;  1 profile
: mark ( a-u) dup #40 + >r 0 begin
  line >10+>  over r@ < 0= until
  nip r> drop ;
: th-r ( y-a) 10* well + ;

\ check well, store into well.
\ h? reuses oob indices as true flag.
: th-b ( b-a) split 10* + well + ;
4 profile
: h? ( bf-f) swap dup split #23 u<
  swap #10 u< and if th-b c@ then or ;
: hit? ( bbbbc-f) 0* h? h? h? h? 0<> ;
: l! ( bc-c) tuck swap th-b c! ;
: lock ( bbbbc-) l! l! l! l! drop ;

\ 4.5.  draw/update shared state.

1 constant &curr \ in(v)alidation
2 constant &next \ queue. set bits
4 constant &well \ to request
8 constant &kept \ redraw.
0 value inval
: iv? ( v-f) inval and ;
: iv! ( v-) inval or to inval ;
: &+ ( v-) dup 1- or ;

\ 'draw' touches the drawn piece to
\ erase next frame. touch a new one
\ to leave the old one on screen.
: touch ( bts-) s0 c! t0 c! b0 ! ;
: drawn ( -bts) b0 @ t0 c@ s0 c@ ;
: curr  ( -bts) b1 @ t1 c@ s1 c@ ;
: curr-r ( -a) b1 1+ c@ th-r ;

5 . \ player update.

\ move piece. kick into turn direction.
: t1@+ ( t-t) t1 c@ + 3 and ;
: go? ( fbt-f) rot 0= if 2drop 0 else:
  over b1 @ +  over t1@+  s1 c@
  piece hit? if 2drop 1 else:
  t1@+ t1 c!  b1 +!  &curr iv! 0 ;
$-100 constant down
: turnkick ( t-) dup>r 0 i go? i i go?
  0 i - i go? down i go? down i + i go?
  down i - r> go? drop ;

\ (g)ameover if entry blocked.
: enter ( -) $1305 b1 ! 0 t1 c! ;
: trykeep ( -) pinned? if else:
  keep enter &kept &curr or iv! ;
: fall? ( -g) 12 %grav !  1 down 0 go?
  0= if 0 else:  kbflush unpin
  curr piece lock  curr-r mark ?dup if
    lines +! 11 %show ! 3 %grav !
  &well else &next &+ then iv!  enter
  qnext  curr touch  curr piece hit? ;
: init ( u-) seed !  well seed well -
  erase  enter qflush  curr touch ;

: tick? ( a-f) -1 over +! c@ 0= ;
: update ( -g) %show c@ if  %show tick?
    if sweep &well &+ iv! then 0
  else:  %grav tick? if fall? else:
  kb $7f and case  0 of endof
  'j' of -1 turnkick endof
  'k' of 1 turnkick endof
  's' of 1 -1 0 go? drop endof
  'f' of 1 1 0 go? drop endof
  'd' of fall? exit endof
  'l' of trykeep endof
  drop 1 exit endcase 0 ;

6 . \ drawing.

$077d value screen   \ bottom left
$db7d value colormem \ of well.

: th-cm ( b-a) split 40* - colormem + ;
6 profile
: p! ( bc-c) tuck swap th-cm c! ;
: plot ( bbbbc-) p! p! p! p! drop ;
: slot ( sb-) dup th-cm 2 - dup 40-
  4 erase 4 erase 0 rot piece plot ;

: sync ( -) [ $c5 lda,# $d012 cmp,
  -5 bne, ] ;  11 profile
: draw ( -) &curr iv?  sync  if
    drawn piece 0* plot
  then  &well iv? if
    well colormem begin
    2dup 10move >10+> 40-
    over vistop < 0= until  2drop
  then  &curr iv? if
    curr piece plot  curr touch
  then  &kept iv? if
    kept@ $060d slot
  then  &next iv? if
    1 th-q c@ $100d slot
    2 th-q c@ $0d0d slot
    3 th-q c@ $0a0d slot
  then  0 to inval ;

: help ( -) ." cmds: new r(esume)"
  ."    keys: sdf jkl q" cr ;
: hue ( fg $bgbd ) $d020 ! $286 c! ;
: rblank ( au-) $a0 fill ;
: canvas ( -) screen 38 + 21 0 do dup
  19 rblank 40- loop 2+ #10 rblank ;
: dw  0 q+ -1 iv! draw ;
: da  11 0 hue page canvas help dw ;

\ 7.  main loop.

: r  kbrep da begin update draw until ;
: fresh ( -u) $a1 @ 1 or ;
: new  0 pro fresh init r ;
3 init cr help ' help start !

