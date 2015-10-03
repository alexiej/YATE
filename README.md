# YATE
YATE (Yet anoter Text Editor) - WPF RichTextBox Extension

YATE is short project to extend RichTextBox with some features from WebBrowser control. WebBrowser control is very good for editing and parse HTML input from clipboard but it is not compatibile with WPF Layout.

![MainWindow](https://raw.githubusercontent.com/alexiej/YATE/master/YATE.Win/docs/MainWindow.png)

In this situation I would like to present you YATE. 

Yate is based on project Document.Editor from: https://documenteditor.codeplex.com/.
Instead of using HTMLToXMLConverter,the Yate convert direct to FlowDocument. Old conversion doesn't support many features from HTML (like other size types, color types , etc..). 

This is not and never will be full HTML conversion to FlowDocument but it helps to work with this type of input.

In this control you can:

* Paste HTML input from clipboard with formatting, images, tables like in WebBrowser.

![INSERT](https://raw.githubusercontent.com/alexiej/YATE/master/YATE.Win/docs/INSERT.gif)

* Automatically recognize URL
* Paste Images, by one command.
* Resizing Images

![RESIZE](https://raw.githubusercontent.com/alexiej/YATE/master/YATE.Win/docs/RESIZE.gif)

* Many commands for edit RichTextBox.
* Good looking Toolbar with MahApps.Metro style.

## Icons/Style

YATE use style and icons from project Mahapss.Metro. https://github.com/MahApps/MahApps.Metro

# Microsoft Public License (Ms-PL)

Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.

A "contributor" is any person that distributes its contribution under this license.

"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
