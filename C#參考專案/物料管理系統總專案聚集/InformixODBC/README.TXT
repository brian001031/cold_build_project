CSDK Installation on windows with ISWI2.0

MsiExec and Installation Properties

     Windows Installer Installation,repair,and uninstallation can be
     activated from the Command line using the utility MsiExec with
     appropriate command-line options.MsiExec is a part of Windows 
     2000, and InstMsi Installs MsiExec into <SystemFolder> on other 
     Systems.

     For example, here are some common installer actions and ther
     corresponding command line options.

     Install:                      Msiexec.exe  /i  D:\Example.msi
     Uninstall:                    MsiExec.exe  /x  D:\Example.msi
     Install to Network:           MsiExec.exe  /a  D:\Example.msi
     Repair:                       MsiExec.exe  /f  D:\Example.msi
     Advertise (to machine):       MsiExec.exe  /jm D:\Example.msi
     Advertise ( to current user): MsiExec.exe  /ju D:\Example.msi

     MsiExec also accepts a product code instead of a path to an MSI
     Package.

     public properties (those with upper-case names) of an
     installation program can be set at the command line.
     For example, the default user and company names displayed in the
     Identification panel of an installation are  taken from the
     target system's registry and stored in the public installer
     properties USERNAME and COMPANYNAME. To set the user and company
     names at the command line, we enter a command similar to the following.
     (Note that property names are case-sensitive, and that we enclose a 
     property value in quotation marks if the value contains any spaces.)

     MsiExec.exe  /i   “<.msi file path name>”   USERNAME=Default
     COMPANYNAME="<company name"

     Similarly user can specify an alternative value for INSTALLDIR on
     the command line

     MsiExec.exe  /i  “<.msi file path name>”
     INSTALLDIR="<NewLocation path>"

     An administration can prevent a product from being uninstalled by
     setting the ARPNOREMOVE property  to 1 at the command line.
     Similarly, setting ARPNOMODIFY to 1 prevents users from modifying 
     installed files, and setting ARPNOREPAIR  to  1 disables the Repair 
     button in the Windows 2000 Add/Remove Programs Control panel applet.

Silent Installation

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-9

     Logging :-
     =======

     We can create a log file at setup runtime using the /L argument
     to MsiExec.
     For example to store all error and warning messages in a text log
     file, we can use the following command line, which passes the e
     and w arguments to /L

     MsiExec.exe  /i  "<.Msi file path name"
     SERIALNUMBER="<serialnumber>"   KEY="<key>"  /Lew  D:\install.log

     To log every action performed by the setup program,the command is

     MsiExec.exe  /i  ".Msi file path name"
     ERIALNUMBER="<serialnumber>"   KEY="<key>" /L*v  D:\install.log

     Note:- By default, MsiExec creates a log file in a users' Temp
     folder only if an error occurs.

Advertise Installation

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-10

Uninstallation of Client SDK

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-11

Uninstallation of Client SDK from Command line

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-12

Modify the Installation

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-12

Repair the Installation

     Refer Informix Client Products Installation Guide for Unix,Linux
     And Windows,Version 2.5 from the url
     http://www.informix.com/answers/english/docs/25sdk/6638.pdf on
     page 2-13
