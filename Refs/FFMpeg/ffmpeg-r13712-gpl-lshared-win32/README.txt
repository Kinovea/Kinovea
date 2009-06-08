These are unofficial FFmpeg Win32 builds made by Ramiro Polla.

These files were originally hosted at: http://arrozcru.no-ip.org/ffmpeg_builds/
The source code they were built with can also be found on the page above.

If you experience any problems with this build, please report them to:
http://arrozcru.no-ip.org/ffmpeg_forum/

FFmpeg revision number: 13712
FFmpeg license: GPL

configuration: --enable-memalign-hack --enable-avisynth --enable-libxvid \
               --enable-libx264 --enable-libgsm --enable-libfaac \
               --enable-libfaad --enable-liba52 --enable-libmp3lame \
               --enable-libvorbis --enable-libtheora --enable-pthreads \
               --enable-swscale --enable-shared --disable-static --enable-gpl

Build system:

gcc 4.2.3
nasm 2.02
w32api-3.11
mingw-runtime CVS May 18th, 2008
binutils 2.18.50


Extra libraries included:

zlib 1.2.3
    http://www.zlib.net/

$ tar zxfv zlib-1.2.3.tar.gz
$ cd zlib-1.2.3
$ ./configure --prefix=/mingw
$ make
$ make install

pthreads-win32 2.8.0
    ftp://sources.redhat.com/pub/pthreads-win32

$ tar zxfv pthreads-w32-2-8-0-release.tar.gz
$ cd pthreads-w32-2-8-0-release
$ make clean GC-static
$ cp libpthreadGC2.a /mingw/lib
$ cp pthread.h sched.h /mingw/include

libfaac 1.26
    http://www.audiocoding.com/

$ tar zxfv faac-1.26.tar.gz
$ patch -p0 < faac_1.26_01_buildsystem.diff
$ cd faac
$ sh bootstrap
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make LDFLAGS="-no-undefined"
$ make install

libfaad2 2.6.1
    http://www.audiocoding.com/

$ tar zxfv faad2-2.6.1.tar.gz
$ patch -p0 < faad2_2.6.1_01_buildsystem.diff
$ cd faad2
$ sh bootstrap
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make LDFLAGS="-no-undefined"
$ make install

libmp3lame 3.97
    http://www.mp3dev.org/

$ tar zxfv lame-3.97.tar.gz
$ cd lame-3.97
$ patch -p0 < ../lame_3.97_non_pic_objects.diff
$ ./configure --prefix=/mingw --disable-shared --enable-static --disable-frontend --enable-nasm
$ make
$ make install

libogg 1.1.3
    http://www.xiph.org/

$ tar zxfv libogg-1.1.3.tar.gz
$ cd libogg-1.1.3
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make
$ make install

libvorbis 1.1.2
    http://www.xiph.org/

$ tar zxfv libvorbis-1.1.2.tar.gz
$ cd libvorbis-1.1.2
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make
$ make install

libtheora 1.0beta3
    http://www.xiph.org/

$ tar xfvj libtheora-1.0beta3.tar.bz2
$ patch -p0 < theora_1.0beta3_01_sys_types.diff
$ cd libtheora-1.0beta3
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make
$ make install

libgsm 1.0.12
    http://kbs.cs.tu-berlin.de/~jutta/toast.html

$ tar zxfv gsm-1.0.12.tar.gz
$ patch -p0 < gsm_1.0-pl12_01_ansi_pedantic.diff
$ cd gsm-1.0-pl12
$ make
$ cp lib/libgsm.a /mingw/lib/
$ cp inc/gsm.h /mingw/include/

libamr-nb 7.0.0.1
    http://www.penguin.cz/~utx/amr

$ tar xfvj amrnb-7.0.0.1.tar.bz2
$ cd amrnb-7.0.0.1
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make
$ make install

libamr-wb 7.0.0.2
    http://www.penguin.cz/~utx/amr

$ tar xfvj amrwb-7.0.0.2.tar.bz2
$ cd amrwb-7.0.0.2
$ ./configure --prefix=/mingw --enable-static --disable-shared
$ make
$ make install

liba52 0.7.4
    http://liba52.sourceforge.net

$ tar zxfv a52dec-0.7.4.tar.gz
$ cd a52dec-0.7.4
$ ./configure --prefix=/mingw
$ make
$ make install

xvidcore 1.1.3
    http://www.xvid.org/downloads.html

$ tar xfvj xvidcore-1.1.3.tar.bz2
$ cd xvidcore-1.1.3/build/generic
$ ./configure --prefix=/mingw
$ make
$ make install
$ rm /mingw/lib/xvidcore.dll
$ mv /mingw/lib/xvidcore.a /mingw/lib/libxvidcore.a

x264 r803
    http://developers.videolan.org/x264.html

$ tar xfvj x264-803.tar.bz2
$ patch -p0 < x264_01_heap_analysis.diff
$ cd x264-803
$ ./configure --prefix=/mingw
$ make
$ make install
