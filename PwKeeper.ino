#include <Arduino.h>
#include <stdio.h>
#include <EEPROM.h>
#include <U8glib.h>
#include <Keyboard.h>
#include "OneButton.h"
#include <avr/pgmspace.h>

#define  PIN_BTN_UP       A9      // Button UP
#define  PIN_BTN_DN       A10     // Button DOWN
#define  RXLED            17      // RX led pin

//                        0x01-0x0E - Fast key access    
#define  BTN_UP           0x20    // UP
#define  BTN_DOWN         0x21    // DOWN
#define  BTN_ENT          0x22    // Enter == DoubleUp
#define  BTN_ESC          0x23    // Esc == DoubleDown
#define  BTN_INS          0x24    // Ins == LongUp
#define  BTN_DEL          0x25    // Del = LongDown
#define  MAX_DATA         1000    // Max data size
#define  MAX_PASSWORD     20      // Max password length
#define  MAX_BUFFER       128     // Max IO size
#define  MAX_EDIT         64      // EditStr size. <= MAX_BUFFER

#define  VERSION       	  "1.5"   // Version string

typedef unsigned long DWORD;

/*
 START
 * RESET - LongPressDOWN
 * PASSWORD - Enter password   
   * SELECT - Up, Down
     * SendPassword - Enter, Ins
     * SelectRecord - Up, Down
     * ConfigMenu   - Del
       * M_CFG_USB
       * M_CFG_PASSW
       * M_CFG_RESET
     * EditRecord   - Esc
       * Select Tag - Up, Down
       * Return     - Esc
       * EditTag    - Enter
         * Select Char - Up, Down
         * Edit char   - Enter, Ins
         * Return      - Esc     
 */

class CGlobalData
{   
  public:
    typedef   void (*pFUNC)( void );    
    
    byte    _St;
    byte      mPassword[MAX_PASSWORD];// 0x20:Up, 0x10:Double, 0x0F:ClickN
    byte      mData[MAX_DATA];  // EEProm data. 00:Nothing, 01:ShowName, 02:SendName, 03:SendPassw, 04:SendCite
    byte    _Wr;                // Data between _St and _Wr stored in flash memory. _Wr == crc8 of block
        
    byte      mChanged;         // mData changed    
    byte      mIsPw;            // Password checked
    short     mRecOffset;       // Record ofset in mData
    byte      mKey;             // Pressed key
    byte      mKeyUpLp;         // In key UP LongPress
    byte      mKeyDnLp;         // In key Dn LongPress
    byte      mPosMenu;         // current selected item
    byte      mPosEdit;         // Position in mEditStr
    byte      mPass;            // loop() pass counter
    byte      mWaitKey;         // Wait key before call to proc
    byte      mRecCur;          // Current Record No
    byte      mRecAll;          // Total Record count
    short     mUsbRx;           // Usb RXed data
    short     mUsbTx;           // Usb TXed data
    long      mIdleTo;
    char      mEditStr[MAX_BUFFER];// Edit buffer for value correction    
    byte    _En;
    
    OneButton mBtUp, mBtDn;     // Button handlers
    pFUNC     mpfRun, mpfRunPrev;
    pFUNC     mpfDisp, mpfDispPrev;

    CGlobalData() : mpfRun(0), mpfRunPrev(0), mpfDisp(0), mpfDispPrev(0),
                    mBtUp( PIN_BTN_UP, true), mBtDn( PIN_BTN_DN, true)
    { memset( &_St, 0, &_En-&_St );      
    }

    void  SetFunc( pFUNC fRun, pFUNC fDisp ) 
          {   if( !fRun ) 
              {  mpfRun = mpfRunPrev; 
              } else 
              {  mpfRunPrev = mpfRun; 
                 mpfRun = fRun;                
              }
              if( !fDisp ) mpfDisp = mpfDispPrev; 
              else {  mpfDispPrev = mpfDisp; 
                      mpfDisp = fDisp; 
              }              
          }
    void  Start(void);
    void  Run(void);
    void  Draw(void); 
    void  FlashRd(void);
    void  FlashWr(void);
    void  SetRecCntr( void );
    void  DrawUsb( const char *Pfx );
    void  DrawYN( const __FlashStringHelper *Msg );
    void  DrawLabels( const __FlashStringHelper *UpStr, const __FlashStringHelper *DnStr );
    void  PrnPrm( int PosN, int X, const __FlashStringHelper *Name );
    void  SerTx( int TxSz, const void *Tx, char *Hdr=0 );
    int   GetFastButton( void );
    int   GetLine( void );
    int   DelCurRec( void );

    static void  fStart(void);  // Start point
    static void  fPassword(void); static void  fDispPassword(void);   // Enter&check main password
    static void  fReset(void);    static void  fDispReset(void);      // Reset all Yes/No?
    static void  fSelect(void);   static void  fDispSelect(void);     // Select record
    static void  fRecMenu(void);  static void  fDispRecMenu(void);    // Select Record tag
    static void  fRecTagPos(void);static void  fDispRecTagPos(void);  // Select position in tag
    static void  fRecTagEdt(void);static void  fDispRecTagEdt(void);  // Edit char in tag
    static void  fRecDel(void);   static void  fDispRecDel(void);     // Delete record Yes\No?
    static void  fRecSave(void);  static void  fDispRecSave(void);    // Save changes Yas\No?
    static void  fCfgMenu(void);  static void  fDispCfgMenu(void);    // Config menu
    static void  fCfgPwd(void);   static void  fDispCfgPwd(void);     // Get new password
    static void  fCfgPwd2(void);  static void  fDispCfgPwd2(void);    // Repeat new password & set
    static void  fUsbIo(void);    static void  fDispUsbIo(void);      // External control
    static void  fTty(void);      static void  fDispTty(void);        // External TTY console
        
    void  SetEditStr( void );
    int   PwdEnter( void );
    void  KbdSend( void );
    void  Defaults( void )
          { memset( &_St, 0, &_En-&_St );
            FlashWr();
          }    
};

static CGlobalData gD;
U8GLIB_SSD1306_128X32 u8g(U8G_I2C_OPT_DEV_0|U8G_I2C_OPT_NO_ACK|U8G_I2C_OPT_FAST);  // Fast I2C / TWI 
const byte PinNoFast[] = { 4,5,6,7,8,16,14,15,18,19,20,21,0 };

//-----------------------------------------------------------------------------

void setup(void) 
{ gD.Start();
}

void loop(void) 
{ gD.Run();
}

// Button handlers
void hBtUpSingle()  { gD.mKey = BTN_UP; }
void hBtUpDouble()  { gD.mKey = BTN_ENT; }
void hBtUpLongSt()  { gD.mKeyUpLp = 1; gD.mKey = BTN_INS; }
void hBtUpLongEnd() { gD.mKeyUpLp = 0; }
void hBtDnSingle()  { gD.mKey = BTN_DOWN; }
void hBtDnDouble()  { gD.mKey = BTN_ESC; }
void hBtDnLongSt()  { gD.mKeyDnLp = 1; gD.mKey = BTN_DEL; }
void hBtDnLongEnd() { gD.mKeyDnLp = 0; }

//-----------------------------------------------------------------------------

int   CGlobalData::GetFastButton( void )
{ int i, n;

  for( i=0; PinNoFast[i]; i++ )
   if( LOW == digitalRead( PinNoFast[i] ) ) break;   // read the input pin

  if( !PinNoFast[i] ) return 0;
  
  for( n=0; n<10; n++ )
  { delay( 50 );
    if( LOW != digitalRead( PinNoFast[i] ) ) break;   // read the input pin
  }
  
  if( n < 4 ) return 0;
  if( n < 8 ) return i+1;   
  
  return (i+1) | 0x10;   
}

//-----------------------------------------------------------------------------

void  CGlobalData::Start(void) 
{ FlashRd();  // Read data from eeprom
  mIdleTo = millis();
//  Serial.begin( 115200 );

  for( int i=0; PinNoFast[i]; i++ )
   pinMode( PinNoFast[i], INPUT_PULLUP ); // set pin to input  

  pinMode(RXLED, OUTPUT);  // Set RX LED as an output
  
  // flip screen, if required
  //u8g.setRot180();
  u8g.setColorIndex(1);

  mBtUp.setClickTicks( 300 );
  mBtDn.setClickTicks( 300 );
  mBtUp.attachClick( hBtUpSingle );
  mBtUp.attachDoubleClick( hBtUpDouble );
  mBtUp.attachLongPressStart( hBtUpLongSt );
  mBtUp.attachLongPressStop( hBtUpLongEnd );
  mBtDn.attachClick( hBtDnSingle );
  mBtDn.attachDoubleClick( hBtDnDouble );
  mBtDn.attachLongPressStart( hBtDnLongSt );
  mBtDn.attachLongPressStop( hBtDnLongEnd );
  
  mWaitKey = 0;
  SetFunc( fStart, 0 );
  SetRecCntr();
}

//-----------------------------------------------------------------------------

void  CGlobalData::Run(void) 
{ mPass++;
  mKey = 0;
  mBtUp.tick();
  mBtDn.tick();

  if( mKey ) mIdleTo = millis();
  if( millis() - mIdleTo > 600*1000L ) 
  { mWaitKey = 0;
    SetFunc( fStart, 0 );
  }

  while( mpfRun )
  { if( mWaitKey && !mKey ) 
    { if( mpfRun != fSelect ) break;
      mKey = GetFastButton();
      if( !mKey ) break;
    }
    mWaitKey = 1;
    mpfRun();
    break;
  }
  
  // picture loop
  u8g.firstPage();  
  do 
  { u8g.setFont( u8g_font_9x18B );
    if( mPass & 0x10 ) u8g.drawBox( 126, 30, 2, 2 ); // Flasher
    if( mpfDisp ) mpfDisp();
  } while( u8g.nextPage() );
}

//-----------------------------------------------------------------------------

void CGlobalData::fStart(void)
{ TXLED0;
  digitalWrite(RXLED, HIGH);
  gD.mPosEdit = 0;
  gD.mIdleTo = millis();
  memset( gD.mEditStr, 0, sizeof(gD.mEditStr) );

  for( int i=0; i < MAX_PASSWORD; i++ )
  { if( gD.mPassword[i] == 0 ) continue;
    gD.SetFunc( gD.fPassword, gD.fDispPassword );
    return;
  }
  gD.SetFunc( gD.fSelect, gD.fDispSelect ); // Password == NULL
}

//-----------------------------------------------------------------------------

void CGlobalData::fPassword(void)
{ int   i = gD.PwdEnter();

  if( !i ) return;
  if( i < 0 )
  { gD.SetFunc( gD.fReset, gD.fDispReset );
    return;
  }

  if( memcmp( gD.mPassword, gD.mEditStr, MAX_PASSWORD ) ) 
  { gD.mWaitKey = 0;
    gD.SetFunc( gD.fStart, 0 ); // Passw bad, restart
    return;
  }
  gD.SetFunc( gD.fSelect, gD.fDispSelect ); // Password Ok
}

void CGlobalData::fDispPassword(void)
{ int   i, n, x, y;
  char  Tb[MAX_PASSWORD+2];

  u8g.drawStr( 20, 13, F("Password?") );
  u8g.setFont(u8g_font_6x12r);
  u8g.drawStr( 110, 7, F(VERSION) ); 
    
  for( i=0; i<MAX_PASSWORD; i++ )
  { n = gD.mEditStr[i];        
    Tb[i] = '0' | (n & 0x0F);
    if( n )
    { x = i*6+1;
      y = n & 0x20 ? 19:29;
      if( n & 0x10 ) u8g.drawBox( x, y+1, 3, 2 );
      else u8g.drawLine( x, y, x+3, y );
    }
  }
  Tb[MAX_PASSWORD] = 0;
  u8g.drawStr( 0, 28, Tb );
}

//-----------------------------------------------------------------------------

void CGlobalData::fReset(void)
{  switch( gD.mKey )
  {  case BTN_ENT:
     case BTN_INS:
        gD.Defaults();
        gD.SetFunc( gD.fStart, 0 );
        break;
     case BTN_DOWN:
     case BTN_ESC:
     case BTN_DEL:
        gD.SetFunc( 0, 0 );
  }
}

void CGlobalData::fDispReset(void)
{ gD.DrawYN( F("Clear all?") );  
}

//-----------------------------------------------------------------------------

void CGlobalData::fSelect(void)
{ int   i, n;
  switch( gD.mKey )
  {  case BTN_UP:
        for( i = gD.mRecOffset-1; i >= 0; i-- )
        { if( gD.mData[i] != 0x01 ) continue;
          gD.mRecOffset = i;
          gD.SetRecCntr();
          break;
        }
        break;
     case BTN_DOWN:
        if( !gD.mData[gD.mRecOffset] ) break; // End of record
        for( i = gD.mRecOffset+1; i < sizeof(gD.mData); i++ )
        { if( gD.mData[i] && gD.mData[i] != 0x01 ) continue;
          gD.mRecOffset = i;
          gD.SetRecCntr();
          break;
        }
        break;
     case BTN_ENT:
     case BTN_INS:
        gD.KbdSend(); // Send password
        break;    
     case BTN_ESC:
        gD.mChanged = 0;
        gD.mPosMenu = 0;            
        gD.SetFunc( gD.fRecMenu, gD.fDispRecMenu );
        break;
     case BTN_DEL:
        gD.mPosMenu = 0;
        memset( gD.mEditStr, 0, sizeof(gD.mEditStr) );
        gD.SetFunc( gD.fCfgMenu, gD.fDispCfgMenu );
        return;
        
     default: // mKey == RecordNo     
        for( i=n=gD.mRecOffset=0; gD.mRecOffset < sizeof(gD.mData); gD.mRecOffset++ )
        { switch( gD.mData[gD.mRecOffset] )
          { default: continue;
            case 0x01:
              i = gD.mRecOffset;
              if( ++n != (gD.mKey&0x0F) ) continue;
              if( gD.mKey & 0x10 ) gD.KbdSend();
              break;
            case 0x00:
              gD.mRecOffset = i;
              break;
          }
          gD.SetRecCntr();
          return;
        }
  }
}

void CGlobalData::fDispSelect(void)
{   int i = gD.mRecOffset, n = 0, x;
    char  Tb[16];

    memset( gD.mEditStr, 0, sizeof(gD.mEditStr) );
    if( gD.mData[i] ) 
    { for( i++; i<sizeof(gD.mData); i++ )
      { x = gD.mData[i] & 0x0FF;        
        if( x <= 0x01 ) break;
        if( x < 0x0A ) n = MAX_EDIT+1;
        if( n < MAX_EDIT )
          gD.mEditStr[n++] = x;
      }
      if( x ) u8g.drawBox( 121, 30, 4, 2 );
    }
    gD.mEditStr[32] = 0;
    u8g.drawStr( 0, 19, gD.mEditStr );
    if( gD.mRecOffset ) u8g.drawBox( 121, 0, 4, 2 );
    u8g.setFont(u8g_font_6x12r);
    sprintf( Tb, "%d/%d", gD.mRecCur, gD.mRecAll );
    u8g.drawStr( 0, 30, Tb ); 
}

//-----------------------------------------------------------------------------

void CGlobalData::fRecMenu(void)
{ switch( gD.mKey )
  {  case BTN_UP:
        if( ++gD.mPosMenu > 3 ) gD.mPosMenu = 0;
        gD.SetEditStr();
        break;
     case BTN_DOWN:
        if( --gD.mPosMenu > 3 ) gD.mPosMenu = 3;
        gD.SetEditStr();
        break;
     case BTN_INS:
     case BTN_ENT:
        if( gD.mPosMenu == 3 ) 
        { gD.SetFunc( gD.fRecDel, gD.fDispRecDel ); 
          break;
        }        
        gD.mPosEdit = 0;
        gD.SetFunc( gD.fRecTagPos, gD.fDispRecTagPos );
        break;
     case BTN_DEL:
     case BTN_ESC:
        if( !gD.mChanged ) gD.SetFunc( gD.fSelect, gD.fDispSelect );
        else gD.SetFunc( gD.fRecSave, gD.fDispRecSave );
        break;
  }
}

void CGlobalData::fDispRecMenu(void)
{   int i = gD.mPosEdit > 13  ? gD.mPosEdit-13:0;
    u8g.drawStr( 1, 13, gD.mEditStr+i );

    u8g.setFont(u8g_font_6x12r);
    gD.PrnPrm( 0,  0, F("Show") );
    gD.PrnPrm( 1, 32, F("Name") );
    gD.PrnPrm( 2, 64, F("Pass") );
    gD.PrnPrm( 3, 96, F("DEL!") );
}

//-----------------------------------------------------------------------------

void CGlobalData::fRecTagPos(void)
{ int   i, n;
  byte  b;

  switch( gD.mKey )
  {  case BTN_UP:
        if( gD.mPosEdit >= MAX_EDIT-1 ) break;
        if( gD.mEditStr[gD.mPosEdit] ) gD.mPosEdit++;
        else gD.mEditStr[gD.mPosEdit+1] = 0;
        break;
     case BTN_DOWN:
        if( --gD.mPosEdit > MAX_EDIT ) gD.mPosEdit = strlen(gD.mEditStr);
        break;
     case BTN_INS:
     case BTN_ENT:
        gD.SetFunc( gD.fRecTagEdt, gD.fDispRecTagEdt );
        break;
     case BTN_ESC:      
        gD.mPosEdit = 0;
        gD.SetFunc( gD.fRecMenu, gD.fDispRecMenu );        

        i = gD.mRecOffset;
        for( ; i < sizeof(gD.mData); i++ )
        { b = gD.mData[i];
          if( !b ) break;
          if( b != gD.mPosMenu+1 ) 
          { if( b == 0x01 ) break;
            continue;
          }          
          for( n = i+1; n < sizeof(gD.mData); n++ ) 
          { b = gD.mData[n];
            if( b <= 0x01 ) break;
          }
          if( n < sizeof(gD.mData) )
            memcpy( gD.mData+i, gD.mData+n, sizeof(gD.mData)-n );
          break;
        }
        n = strlen( gD.mEditStr )+1;
        if( i+n > sizeof(gD.mData) ) n = sizeof(gD.mData) - i;
        if( n > 0 ) memmove( gD.mData+i+n, gD.mData+i, sizeof(gD.mData)-(i+n) );
        gD.mData[i++] = gD.mPosMenu+1;
        memcpy( gD.mData+i, gD.mEditStr, n-1 );
        break;
     case BTN_DEL:
        gD.mPosEdit = 0;
        gD.SetFunc( 0, 0 );
        break;
  }
}

void CGlobalData::fDispRecTagPos(void)
{ int i = gD.mPosEdit > 13 ? 13:gD.mPosEdit;
  u8g.drawFrame( i*9, 0, 11, 16 );
  gD.fDispRecMenu();
}

//-----------------------------------------------------------------------------

void CGlobalData::fRecTagEdt(void)
{ byte  c;
  int   i, n;

  switch( gD.mKey )
  {  case BTN_UP:
        c = gD.mEditStr[gD.mPosEdit] + 1;
        if( c < ' ' ) c = ' ';
        else if( c > 0xFF ) c = ' ';
             else if( c == 0x7F ) c = 0xC0;
        gD.mEditStr[gD.mPosEdit] = c;
        gD.mChanged = 1;
        break;
     case BTN_DOWN:
        c = gD.mEditStr[gD.mPosEdit] - 1;
        if( c < ' ' ) c = 0xFF;
        else if( c == 0xBF ) c = 0x7E;            
        gD.mEditStr[gD.mPosEdit] = c;
        gD.mChanged = 1;
        break;
     case BTN_INS:
        if( gD.mPosEdit >= MAX_EDIT-1 ) break;
        i = strlen( gD.mEditStr );
        if( i >= MAX_EDIT-1 ) i = MAX_EDIT-1;
        i -= gD.mPosEdit;
        if( i > 0 ) memmove( gD.mEditStr+gD.mPosEdit+1, gD.mEditStr+gD.mPosEdit, i );
        gD.mEditStr[gD.mPosEdit] = 'A';
        gD.mChanged = 1;
        break;
     case BTN_DEL:
        strcpy( gD.mEditStr+gD.mPosEdit, gD.mEditStr+gD.mPosEdit+1 );
        gD.mChanged = 1;
        break;
     case BTN_ENT:
     case BTN_ESC:      
        gD.SetFunc( gD.fRecTagPos, gD.fDispRecTagPos );
        break;
  }
}

void CGlobalData::fDispRecTagEdt(void)
{   if( gD.mPass & 0x10 ) 
    { int i = gD.mPosEdit > 13 ? 13:gD.mPosEdit;
      u8g.drawFrame( i*9, 0, 11, 16 );
    }
    gD.fDispRecMenu();
}

//-----------------------------------------------------------------------------

void CGlobalData::fRecSave(void)
{ switch( gD.mKey )
  {  case BTN_UP:
        return;
     case BTN_ENT:
     case BTN_INS:
        gD.FlashWr();
        break;
     case BTN_DOWN:
     case BTN_ESC:
     case BTN_DEL:
        gD.mChanged = 0;
        gD.FlashRd();
        break;
  }
  gD.SetRecCntr();
  gD.SetFunc( gD.fSelect, gD.fDispSelect );
}

void CGlobalData::fDispRecSave(void)
{   gD.DrawYN( F("Save changes?") );      
}

//-----------------------------------------------------------------------------

void CGlobalData::fRecDel(void)
{ switch( gD.mKey )
  {  case BTN_ENT:
     case BTN_INS:
        if( !gD.DelCurRec() ) break;        
     case BTN_DOWN:
     case BTN_ESC:
     case BTN_DEL:      
        gD.SetFunc( gD.fRecMenu, gD.fDispRecMenu );
        break;
  }
}

void CGlobalData::fDispRecDel(void)
{ u8g.drawStr( 0, 20, gD.mEditStr );
  gD.DrawYN( F("Delete record?") );  
}

int  CGlobalData::DelCurRec(void)
{ int i, n;

  if( !gD.mData[gD.mRecOffset] ) return 0;
  for( i = gD.mRecOffset+1; gD.mData[i]>0x01 && i<sizeof(gD.mData); i++ );
  n = sizeof(gD.mData) - i;
  if( n <= 0 ) return 0;
  memcpy( gD.mData+gD.mRecOffset, gD.mData+i, n );
  memset( gD.mData+gD.mRecOffset+n, 0, sizeof(gD.mData)-(gD.mRecOffset+n) );
  gD.mChanged = 1;
  return 1;
}

//-----------------------------------------------------------------------------

void CGlobalData::fCfgMenu(void)
{ switch( gD.mKey )
  {  case BTN_UP:
        if( ++gD.mPosMenu > 3 ) gD.mPosMenu = 0;
        break;
     case BTN_DOWN:
        if( --gD.mPosMenu > 3 ) gD.mPosMenu = 3;
        break;
     case BTN_INS:
     case BTN_ENT:
        gD.mPosEdit = 0;
        switch( gD.mPosMenu )
        { case 0: // Enter USB exchange mode
            gD.mIsPw = 0;
            gD.mWaitKey = 0;
            gD.mUsbRx = gD.mUsbTx = 0;
            gD.SetFunc( gD.fUsbIo, gD.fDispUsbIo );
            break;
            
          case 1: // Enter TTY exchange mode
            gD.mWaitKey = 0;
            gD.mUsbRx = gD.mUsbTx = 0;
            gD.SetFunc( gD.fTty, gD.fDispTty );
            break;
            
          case 2:
            memset( gD.mEditStr, 0, sizeof(gD.mEditStr) );
            gD.SetFunc( gD.fCfgPwd, gD.fDispCfgPwd ); 
            break;
            
          case 3: 
            gD.SetFunc( gD.fReset, gD.fDispReset ); 
            break;
        }
        break;
     case BTN_DEL:
     case BTN_ESC:        
        if( !gD.mChanged ) gD.SetFunc( gD.fSelect, gD.fDispSelect );
        else gD.SetFunc( gD.fRecSave, gD.fDispRecSave );
        break;
  }
}

void CGlobalData::fDispCfgMenu(void)
{   u8g.drawStr( 1, 14, gD.mEditStr );
    u8g.setFont(u8g_font_6x12r);
    gD.PrnPrm( 0,  0, F("USB") );
    gD.PrnPrm( 1, 32, F("TTY") );
    gD.PrnPrm( 2, 64, F("Pass") );
    gD.PrnPrm( 3, 96, F("CLR!") );
}

//-----------------------------------------------------------------------------

void CGlobalData::fCfgPwd(void)
{ int i = gD.PwdEnter();
  if( i < 0 ) 
  { gD.SetFunc( gD.fCfgMenu, gD.fDispCfgMenu ); 
    return;
  }
  if( !i ) return;
  gD.mPosEdit = 0;
  memcpy( gD.mEditStr+32, gD.mEditStr, MAX_PASSWORD );
  memset( gD.mEditStr, 0, MAX_PASSWORD );
  gD.SetFunc( gD.fCfgPwd2, gD.fDispCfgPwd2 ); 
}

void CGlobalData::fDispCfgPwd(void)
{   gD.fDispPassword();
    u8g.setFont(u8g_font_6x12r);
    u8g.drawStr( 0, 7, F("New") );   
}

//-----------------------------------------------------------------------------

void CGlobalData::fCfgPwd2(void)
{ int i = gD.PwdEnter();
  if( i < 0 ) 
  { gD.SetFunc( gD.fCfgMenu, gD.fDispCfgMenu ); 
    return;
  }
  if( !i ) return;
  
  if( memcmp( gD.mEditStr+32, gD.mEditStr, MAX_PASSWORD ) ) 
  { gD.SetFunc( gD.fCfgMenu, gD.fDispCfgMenu ); 
    return;
  }
  gD.mChanged = 1;
  memcpy( gD.mPassword, gD.mEditStr, MAX_PASSWORD );
  gD.SetFunc( gD.fRecSave, gD.fDispRecSave );   
}

void CGlobalData::fDispCfgPwd2(void)
{   gD.fDispPassword();
    u8g.setFont(u8g_font_6x12r);
    u8g.drawStr( 0, 7, F("Rpt") );   
}

//-----------------------------------------------------------------------------
// Packet: <Type(AN1)><Seq_0x40+Offset(AN1)><Data(AN?)><Crc8_0x40+0x3F(AN1)><\n>
void CGlobalData::fUsbIo(void)
{ byte    c;
  int     i, n;
      
  while( 1 )
  { i = gD.GetLine();    
    if( i <= 0 ) break;    
  
    if( gD.mPosEdit < 3 )
    { gD.SerTx( -1, "XAPwKp" );
      continue;
    }

    c = 0;
    for( i=0; i<gD.mPosEdit-1; i++ ) c = crc8( c, gD.mEditStr[i] );
    
    gD.mPosEdit = 0;    
    if( (c&0x3F) != (gD.mEditStr[i]&0x3F) )
    { // Crc Failed. Echo back
      gD.mEditStr[0] = 'C';
      gD.SerTx( i, 0 );
      return;
    }

    // Crc OK.
    gD.mEditStr[i] = 0;
    gD.mEditStr[0] |= 0x20;
    
    switch( gD.mEditStr[0] )
    { default:  
       if( gD.mIsPw ) break;
       goto gErr;
      case 's':
      case 'p': // Password op
        for( n=0; n<MAX_PASSWORD; n++ )
          gD.mEditStr[40+n] = gD.mEditStr[2+n] & 0x3F;
        break;
    }
    
    switch( gD.mEditStr[0] )
    { case 's':  // Set Password <S00><NewPassword>
        memcpy( gD.mPassword, gD.mEditStr+40, sizeof(gD.mPassword) );
        memset( gD.mData, 0, MAX_DATA );
      case 'p':  // Password     <Px><OldPassword>
        delay( 1500 );
        if( !memcmp( gD.mPassword, gD.mEditStr+40, sizeof(gD.mPassword) ) )
        { gD.mIsPw = 1;  
        } else 
        { gD.mIsPw = 0;
          gD.mEditStr[0] = 'L';        
        }
        break;
      case 'r':  // Read     <Rx>    
        gD.mEditStr[2] = 0;
        gD.SerTx( MAX_DATA, gD.mData, gD.mEditStr );
        return;
      case 'w':  // Write     <Wx><Data> Offset = (x&0x3F)*120
        gD.mChanged = 1;
        n = gD.mEditStr[1] & 0x0F;
        n *= 120;
        if( n >= MAX_DATA ) break;
        if( n+i >= MAX_DATA ) i = MAX_DATA-n;        
        if( i > 2 ) memcpy( gD.mData+n, gD.mEditStr+2, i-2 );
        break;
      case 'f':  // Flush EPROM     <Fxx>
        gD.FlashWr();
        break;
gErr:
      default:
        gD.mEditStr[0] = 'L';
        break;
    }
    gD.SerTx( i, 0 );
    return;
  }
}

void CGlobalData::fDispUsbIo(void)
{ gD.DrawUsb( "USB" );  
}

void  CGlobalData::DrawUsb( const char *Pfx )
{ char  Tb[32];

  if( gD.mUsbRx < 0 ) sprintf( Tb, "%s: Offline", Pfx );
  else sprintf( Tb, "%s: %d/%d", Pfx, mUsbRx, mUsbTx );
  u8g.drawStr( 1, 19, Tb );
  u8g.setFont(u8g_font_6x12r);
  DrawLabels( F(VERSION), F("Stop") );
  //sprintf( Tb, "Ph: %d/%02X", gD.mRecNo, gD.mEditStr[0] );
  //u8g.drawStr( 1, 30, Tb );
}

void CGlobalData::SerTx( int TxSz, const void *Tx, char *Hdr )
{   int   i, c = 0;
    char  Tb[8];

    mPosEdit = 0;
    if( !Tx ) Tx = mEditStr;
    if( TxSz < 0 ) TxSz = strlen( (char*)Tx );

    while( Hdr && *Hdr )
    { Serial.write( Hdr, 1 );
      c = crc8( c, *Hdr++ );
      mUsbTx++;
    }
    
    for( i=0; i<TxSz; i++ ) c = crc8( c, ((char*)Tx)[i] );

    c &= 0x3F;
    Tb[0] = 0x40 | c;
    Tb[1] = '\n';
    Serial.write( (char*)Tx, i );
    Serial.write( Tb, 2 );
    mUsbTx += i+2;
}
//-----------------------------------------------------------------------------
// Packet: <Cmd><\n>
// Cmd: U-GoFirst, D-GoLast, u-up, d-down
//

const char MapTag[] = { 'E','C','N','P','1','2','3','4','5','6' };

void CGlobalData::fTty(void)
{ byte    c;  
  int     n, i, j;
    
  while( 1 )
  { i = gD.GetLine();
    if( i <= 0 ) break;

    Serial.print( gD.mEditStr );
    gD.mUsbTx += strlen( gD.mEditStr );

    i = gD.mRecOffset;        
    switch( gD.mEditStr[0] )
    {  case 'U': 
        gD.mRecOffset = 0;
        break;

       case 'u': 
        if( gD.mRecOffset > 0 ) gD.mRecOffset--;
        for( ; gD.mRecOffset > 0 && gD.mData[gD.mRecOffset]!=0x01; gD.mRecOffset-- );
        break;
      
       case 'd': 
        if( gD.mData[i] ) i++;
        for( ; i < MAX_DATA && gD.mData[i]; i++ ) if( gD.mData[i]==0x01 ) { gD.mRecOffset = i; break; }
        break;
        
       case 'A': 
        gD.mChanged = 1;
        i = strlen( (char*)gD.mData );
        if( i > MAX_DATA-16 ) break;
        strcpy( (char*)gD.mData+i, "\x01New" );
        gD.mRecOffset = i;
        break;
        
       case 'D': 
        if( gD.mData[i] ) i++;
        for( ; i < MAX_DATA && gD.mData[i]; i++ ) if( gD.mData[i]==0x01 ) gD.mRecOffset = i;
        break;

      case '?':
        Serial.print( F("\n?,Move:U,u,D,d,SetMainPass:W,AddRec:A,DelRec:E,Set Cmnt:C,Name:N,Pass:P,Val:1-9") );
        break;

      case 'E':  // Erase
        gD.DelCurRec();
        break;        
      
      case 'W': // Password op
        i = 1;
        j = 0;
        gD.mChanged = 1;
        memset( gD.mPassword, 0, sizeof(gD.mPassword) );
        while( i < gD.mPosEdit )
        { n = 1;
          c = gD.mEditStr[i];
          while (++i < gD.mPosEdit && c == gD.mEditStr[i]) 
                    if( ++n >= 15 ) break;
                    
          switch (c)
          { case 'u': n |= 0x20; break;
            case 'U': n |= 0x30; break;              
            case 'D': n |= 0x10; break;
          }
          gD.mPassword[j] = n;
          if( ++j >= MAX_PASSWORD ) break;
        }
        break;        
      
      default:
        for( i=1; i<sizeof(MapTag); i++ )
        { if( MapTag[i] != gD.mEditStr[0] ) continue;
          n = gD.mRecOffset;
          if( i > 1 ) n++;
          for( ; n<MAX_DATA; n++ )
          { c = gD.mData[n];
            if( c != i && c > 0x01 ) continue;
            if( c == i )
            { for( j=n+1; j<MAX_DATA && gD.mData[j]>0x09; j++ );
              strcpy( (char*)gD.mData+n, (char*)gD.mData+j );
            }
            gD.mChanged = 1;
            if( gD.mPosEdit < 2 ) break;
            
            memmove( gD.mData+n+gD.mPosEdit, gD.mData+n, MAX_DATA-(n+gD.mPosEdit) );
            memcpy( gD.mData+n, gD.mEditStr, gD.mPosEdit );
            gD.mData[n] = i;
            break;
          }
          break;
        }        
    }    

    gD.mUsbTx += 7;
    gD.SetRecCntr();
    Serial.print( F("\nRec") );
    Serial.print( (int)gD.mRecCur );
    Serial.print( F(": ") ); 

    n = 0;
    i = gD.mRecOffset;        
    for( ; i < MAX_DATA && gD.mData[i]; i++ ) 
    { c = gD.mData[i];
      if( c == 0x01 && n++ ) break;
      if( c < 0x0A ) 
      { if( c != 0x01 ) { Serial.write( ',' ); Serial.write( ' ' ); }
        Serial.write( MapTag[c] );
        Serial.write( ':' );
        gD.mUsbTx += 2;
        continue;          
      }
      Serial.write( c );
      gD.mUsbTx++;
    }
    Serial.print( F("\n>") );
    gD.mUsbTx += 2;
    gD.mPosEdit = 0;   
  }
}

void CGlobalData::fDispTty(void)
{ gD.DrawUsb( "TTY" );  
}

//-----------------------------------------------------------------------------

int   CGlobalData::GetLine(void)
{ int     i;

  if( gD.mKey )
  { TXLED0;
    digitalWrite(RXLED, HIGH);    
    gD.mPosMenu = 0;
    gD.mPosEdit = 0;
    gD.mRecOffset = 0;
    memset( gD.mEditStr, 0, sizeof(gD.mEditStr) );
    if( !gD.mChanged ) gD.SetFunc( gD.fSelect, gD.fDispSelect );
    else gD.SetFunc( gD.fRecSave, gD.fDispRecSave );    
    return 0;
  }

  digitalWrite(RXLED, LOW);
  gD.mWaitKey = 0;
  if( !Serial ) 
  { gD.mUsbRx = gD.mUsbTx = -1;
    return 0;
  }
    
  while( 1 )
  { i = Serial.read();
    if( i < 0 ) 
    { TXLED0;
      return 0;
    }

    TXLED1;
    gD.mUsbRx++;  
    if( i == '\n' )
    { gD.mEditStr[gD.mPosEdit] = 0;
      return 1;
    }
    
    gD.mEditStr[gD.mPosEdit] = i;
    
    if( ++gD.mPosEdit < sizeof(gD.mEditStr) ) continue;
    
    gD.mUsbRx = 0;
    gD.mPosEdit = 0;    
  }
}

//-----------------------------------------------------------------------------

void  CGlobalData::DrawYN( const __FlashStringHelper *Msg )
{ if( gD.mPass & 0x10 ) 
      u8g.drawStr( 1, 21, Msg );
  gD.DrawLabels( F("Yes"), F("No") );
}

//-----------------------------------------------------------------------------

void  CGlobalData::DrawLabels( const __FlashStringHelper *UpStr, const __FlashStringHelper *DnStr )
{ int   n;
  PGM_P p;

  u8g.setFont(u8g_font_6x12r);

  p = reinterpret_cast<PGM_P>(UpStr);
  n = 0;
  while( pgm_read_byte(p++) ) n++;  
  u8g.drawStr( 128-n*6, 7, UpStr );
  p = reinterpret_cast<PGM_P>(DnStr);
  n = 0;
  while( pgm_read_byte(p++) ) n++;  
  u8g.drawStr( 128-n*6, 30, DnStr ); 
}
//-----------------------------------------------------------------------------

void  CGlobalData::PrnPrm( int RowN, int X, const __FlashStringHelper *Name )
{ u8g.drawStr( X+2, 30, Name );
  if( mPosMenu == RowN )
    u8g.drawFrame( X, 20, 28, 12 );
}

//-----------------------------------------------------------------------------

int   CGlobalData::PwdEnter(void)
{ char  c;

  if( mPosEdit >= MAX_PASSWORD  ) return 1;
  
  switch( mKey )
  {  case BTN_UP:    c = 0x20; break; // UP pressed          
     case BTN_DOWN:  c = 0x00; break; // DOWN pressed          
     case BTN_ENT:   c = 0x30; break; // ENTER pressed
     case BTN_ESC:   c = 0x10; break; // ESC pressed     
     case BTN_DEL:  // LongDOWN pressed
        return -1;
     case BTN_INS:  // LongUP pressed
        return 1;
  }

  if( mEditStr[mPosEdit] &&
      (mEditStr[mPosEdit] & 0x30) != c 
    ) mPosEdit++; // NextPos.
  mEditStr[mPosEdit] = c | (mEditStr[mPosEdit] + 1);
  return 0;
}

//-----------------------------------------------------------------------------

void  CGlobalData::SetEditStr( void )
{ char  c;
  int   n, i = mRecOffset;

  switch( mPosMenu )
  { case 3:
      strcpy( mEditStr, "<Delete>" );
      return;
    case 2:
    case 1:  i++;
  }
  memset( mEditStr, 0, MAX_EDIT );
  
  for( ; i < sizeof(mData); i++ )
  { c = mData[i];
    if( !c )
        return; // No tag
    
    if( c == mPosMenu+1 ) 
        break;
    
    if( c == 0x01 ||
        (c <= 0x09 && c > mPosMenu+1)
      ) return; // No tag
  }
  
  n = i + 1;
  for( c = 0; n<sizeof(mData) && c<MAX_EDIT-1 && mData[n]>=' '; n++,c++ ) 
    mEditStr[c] = mData[n];  
}

//-----------------------------------------------------------------------------

const byte mapCtrl[] = {  0x00,           //\@ - Release all
                          KEY_LEFT_ALT,   //\a, \A
                          KEY_BACKSPACE,  //\b, \B
                          KEY_LEFT_CTRL,  //\c, \C
                          KEY_DELETE,     //\d, \D
                          KEY_ESC,        //\e, \E
                          KEY_RIGHT_SHIFT,//\f, \F
                          KEY_RIGHT_ARROW,//\g, \G                          
                          KEY_HOME,       //\h, \H
                          KEY_INSERT,     //\i, \I
                          0,              // j
                          KEY_CAPS_LOCK,  //\k. \K
                          KEY_LEFT_ARROW, //\l, \L
                          0,              // m
                          KEY_END,        //\n, \N
                          KEY_PAGE_DOWN,  //\o, \O
                          KEY_PAGE_UP,    //\p, \P
                          KEY_RIGHT_ALT,  //\q, \Q
                          KEY_RETURN,     //\r, \R
                          KEY_LEFT_SHIFT, //\s, \S
                          KEY_TAB,        //\t, \T
                          KEY_UP_ARROW,   //\u, \U
                          KEY_LEFT_GUI,   //\v, \V
                          KEY_DOWN_ARROW, //\w, \W
                          KEY_RIGHT_GUI,  //\x, \X
                          0,              //\y, \Y
                          0               //\z, \Z - Delay
                    };

/* Record format: <Tag(1)><Text(?)>[<Tag(1)><Text(?)>...]
   Tag: 1:DispName,2:Login,3:Pass,4:V1,5:V2,6:V3,7:V4,8:V5,9:V6
   Dispname always exists. All other tags - not necessary.
   Text encoding - Win1251
*/

// Send order: V1,V2,Name,V3,Pass,V4,V5,V6
const byte TxOrder[] = { 0x04, 0x05, 0x02, 0x06, 0x03, 0x07, 0x08, 0x09 }; 
  
void  CGlobalData::KbdSend( void )
{ int   i, n, mode, kbpress, t, ti, e, s = mRecOffset;
  byte  b;

  TXLED1;
  digitalWrite(RXLED, LOW);
  Keyboard.begin();

  // Skip Name
  if( mData[s] == 0x01 )
    for( s++; s<sizeof(mData) && mData[s] > 0x09; s++ );

  // Find EOR    
  for( e=s; e<sizeof(mData) && mData[e] > 0x01; e++ );  // Skip till EOR

  for( ti=0; ti<sizeof(TxOrder); ti++ )
  { mode = 0;
    kbpress = 0;
    t = TxOrder[ti];
    for( i = s; i<e; i++ )
      if( t == mData[i] ) break; // Tag found

    if( i >= e ) continue; // Tag not found
       
    for( i++; i<e; i++ )
    { b = mData[i];
      if( b < 0x0A ) break; // End of tag

      switch( mode )
      { case 0: // Chars
          if( b == '\\' ) mode++;
          else Keyboard.write( b );
          continue;
        
        case 1: // first ch after modificator
          mode = 0;
        case 2:
          switch( b )
          { case '[':
              mode = 2; // Switch to repeat mode
              continue;
            case '\\':
              Keyboard.write( b );
            case ']':
              mode = 0;
              continue;
            case '0':
              Keyboard.write( KEY_F10 );
              continue;
            case ':':
              Keyboard.write( KEY_F11 );
              continue;
            case ';':
              Keyboard.write( KEY_F12 );
              continue;
            case 'z':
              delay(300);
              continue;
            case 'Z':
              delay(1000);
              continue;
            case '@':
              kbpress = 0;
              Keyboard.releaseAll();
              continue;
          }
        
          if( b >= '1' && b <= '9' ) 
          { Keyboard.write( KEY_F1+(b&0x0F)-1 );
            continue;
          }

          n = mapCtrl[ b & 0x1F ];
          if( !n ) continue;
          if( b & 0x20 )
          {   Keyboard.press( n );
              kbpress++;
          } else 
          {   Keyboard.release( n );
              kbpress--;
          }
      } // Switch
    } // One tag
    if( kbpress > 0 ) Keyboard.releaseAll();
  } // tags

  delay(200);
  Keyboard.end();
  delay(200);
  TXLED0;
  digitalWrite(RXLED, HIGH);
}

void CGlobalData::SetRecCntr( void )
{int i;

 mRecAll = 0;
 mRecCur = 0; 
 for( i=0; i<sizeof(mData) && mData[i]; i++ )
 { if( mData[i] != 0x01 ) continue;
   mRecAll++;
   if( i <= mRecOffset ) mRecCur++;
 }
}

//-----------------------------------------------------------------------------

byte crc8( byte crc, byte ch ) 
{  
    for (uint8_t i = 8; i; i--) {
      uint8_t mix = crc ^ ch;
      crc >>= 1;
      if (mix & 0x01 ) crc ^= 0x8C;
      ch >>= 1;
  }
  return crc;
}

//-----------------------------------------------------------------------------

void CGlobalData::FlashRd(void)
{ int   i;
  
  _Wr = 0;
  for( i=0; i < &_Wr - &_St; i++ )
  { (&_St)[i] = EEPROM.read( i );
    _Wr = crc8( _Wr, (&_St)[i] );
  }
  
  i = EEPROM.read( i ) & 0xFF;  
  if( _Wr != i ) Defaults(); // CRC failed. Reset data.
  _Wr = 0;
}

//-----------------------------------------------------------------------------

void CGlobalData::FlashWr(void)
{ int   i;
  _Wr = 0;
  for( i=0; i < &_Wr - &_St; i++ )
  { EEPROM.write( i, (&_St)[i] );
    _Wr = crc8( _Wr, (&_St)[i] );
  }
  EEPROM.write( i, _Wr );
  _Wr = 0;
}

const u8g_fntpgm_uint8_t *ISO8859fonts[] =
{
//   u8g_font_10x20,
   //u8g_font_4x6,
   //u8g_font_5x8,
   //u8g_font_6x10,
   //u8g_font_6x12,
   //u8g_font_8x13B,
   //u8g_font_8x13,
   //u8g_font_8x13O,
   //u8g_font_9x15B,
   //u8g_font_9x15,
   u8g_font_9x18B,
   //u8g_font_9x18,
   NULL
};

