\ \ ex: 22 recent ( list 22 words. )
: id. ( t-) name>string type space ;
:noname ( ut-uf) id. 1- dup 0<> ;
: recent ( u-) literal dowords drop ;
