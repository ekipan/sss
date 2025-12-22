\ \ eg: 22 recent ( list 22 words. )
: crtype ( au-) 38 over - $d3 c@ <
  if cr then type ;
: id. ( t-) name>string crtype space ;
:noname ( ut-uf) id. 1- dup 0<> ;
: recent ( u-) literal dowords drop ;
