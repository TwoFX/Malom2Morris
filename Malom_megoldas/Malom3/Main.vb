' Malom, a Nine Men's Morris (and variants) player and solver program.
' Copyright(C) 2007-2016  Gabor E. Gevay, Gabor Danner
' 
' See our webpage (and the paper linked from there):
' http://compalg.inf.elte.hu/~ggevay/mills/index.php
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program.  If not, see <http://www.gnu.org/licenses/>.


Imports System.Drawing

Public Class FrmMain
    Public _Board As New Board(Me)
    Public Game As Game
    Public Settings As New FrmSettings
    Public Loaded As Boolean
    Public StatusGraphics As Graphics

    <System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute>
    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try

            Rules.Main = Me
            InitRules()

            'SetVariant(RuleVariant.Standard)
            SetVariantToWrapper()

            AddHandler MnuPly1Human.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly1Computer.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly1Perfect.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly2Human.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly2Computer.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly2Perfect.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly1Combined.Click, AddressOf ChangePlayerTypes
            AddHandler MnuPly2Combined.Click, AddressOf ChangePlayerTypes

            Me.SuspendLayout()
            Me.Height = Screen.PrimaryScreen.WorkingArea.Height
            Me.Width = Me.Height - MenuStrip.Height - StatusStrip.Height - StatusStrip1.Height - 21
            Me.Top = 0
            Me.Left = Screen.PrimaryScreen.WorkingArea.Width / 2 - Me.Width / 2
            Me.ResumeLayout()
            _Board.Anchor = AnchorStyles.Top + AnchorStyles.Bottom + AnchorStyles.Left + AnchorStyles.Right
            _Board.Top = MenuStrip.Height
            _Board.Size = New Size(Me.ClientSize.Width, Me.ClientSize.Height - MenuStrip.Height - StatusStrip.Height - StatusStrip1.Height)
            Me.Controls.Add(_Board)

            Dim b = New Bitmap(StatusStrip.Height, StatusStrip.Height)
            StatusGraphics = Graphics.FromImage(b)
            LblKov.Image = b

            Settings.LoadSettings()
            'InitUjEval()

            'Game = New Game(New HumanPlayer, New HumanPlayer, Me)
            'Game = New Game(New HumanPlayer, New ComputerPlayer, Me)
            Game = New Game(New HumanPlayer, New PerfectPlayer, Me)
            'Game = New Game(New ComputerPlayer(False), New ComputerPlayer(True), Me)

            'Game = New Game(New ComputerPlayer, New ComputerPlayer, Me)
            'Game = New Game(New ComputerPlayer, New PerfectPlayer, Me)
            'Game = New Game(New PerfectPlayer, New ComputerPlayer, Me)

            Loaded = True

            'GenerateLookuptables()
        Catch ex As Exception
            MsgBox("Exception in frmMain_Load" & vbCrLf & ex.ToString)
        End Try
    End Sub

    Public Sub GenerateLookuptables()
        SetVariant(RuleVariant.Morabaraba)

        Dim r As String = ""

        'For i = 0 To 23
        '    Dim adj As Integer = 0
        '    For j = 1 To Rules.CSLTáblaGráf(i, 0)
        '        adj = adj Or (1 << CSLTáblaGráf(i, j))
        '    Next
        '    r = r & adj & ","
        'Next

        For i = 0 To Rules.MillPos.GetUpperBound(0)
            Dim mask As Integer = 0
            For j = 0 To 2
                mask = mask Or (1 << Rules.MillPos(i, j))
            Next
            r = r & mask & ","
        Next

        Clipboard.SetText(r)
    End Sub

    'Set the variant to the same as wrappers.dll was built with
    Private Sub SetVariantToWrapper()
        Select Case Wrappers.Constants.Variant
            Case Wrappers.Constants.Variants.std
                SetVariant(RuleVariant.Standard)
            Case Wrappers.Constants.Variants.lask
                SetVariant(RuleVariant.Lasker)
            Case Wrappers.Constants.Variants.mora
                SetVariant(RuleVariant.Morabaraba)
        End Select
    End Sub

    Public Sub NewGame()
        'Debug.Write(New Diagnostics.StackTrace())

        If Not Game.PlayertypeChangingCmdAllowed() Then Return

        Game.CancelThinking()

        Dim prevhistory = Game.history
        Game = New Game(Game.Ply(1), Game.Ply(0), Me)
        Dim a = prevhistory.Last
        While a IsNot Nothing
            Game.history.AddBefore(Game.history.First, a.Value)
            a = a.Previous
        End While
    End Sub

    Private Sub MnuNew_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuNew.Click
        NewGame()
    End Sub

    Public Sub UpdateUI(ByVal s As GameState)
        Game.UpdateLabels()
        _Board.UpdateGameState(s)
    End Sub

    Private Sub MnuUndo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuUndo.Click
        If Not Game.Undo() Then LblKov.Text = "Cannot undo. " & LblKov.Text
    End Sub
    Private Sub MnuRedo_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuRedo.Click
        If Not Game.Redo() Then LblKov.Text = "Cannot redo. " & LblKov.Text
    End Sub

    Private Sub MnuCopy_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuCopy.Click
        Game.copy()
    End Sub
    Private Sub MnuPaste_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuPaste.Click
        Game.paste()
    End Sub

    Delegate Sub DPrintDepth(ByVal sz As String)
    Public Sub PrintDepth(ByVal sz As String)
        LblCalcDepths.Text = "D: " & sz
    End Sub
    Delegate Sub DSetText(ByVal s As String)
    Public Sub SetText(ByVal s As String)
        Me.Text = s
    End Sub
    Delegate Sub DLblPerfEvalSettext(ByVal s As String)
    Public Sub LblPerfEvalSettext(ByVal s As String)
        LblPerfEval.Text = s
    End Sub

    Private Sub MnuSettings_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuSettings.Click
        Settings.Show()
    End Sub

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        For Each p As Player In Game.Plys
            p.Quit()
        Next
    End Sub

    Private Sub MnuSwitchSides_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MnuSwitchSides.Click
        Game.SwitchPlayers()
    End Sub

    <System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute>
    Private Sub frmMain_KeyPress(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles Me.KeyPress
        Try
            If e.KeyChar = " " Then
                _Board.ShowLastJeloltMezok()
            End If
            If e.KeyChar = "m" Then
                Game.Ply(0) = New UDPPlayer
            End If
            If e.KeyChar = "M" Then
                Game.Ply(0) = New UDPPlayer
                Game.Ply(1) = New ComputerPlayer
                Game.SwitchPlayers()
            End If
            If e.KeyChar = "a" Then
                _Board.SwitchAdvisor()
            End If
        Catch ex As Exception
            MsgBox("Exception in frmMain_KeyPress" & vbCrLf & ex.ToString)
        End Try
    End Sub

    Private Sub ChangePlayerTypes(ByVal sender As System.Object, ByVal e As System.EventArgs)
        If sender.checked Then Return
        If Not Game.PlayertypeChangingCmdAllowed() Then Return
        If Object.ReferenceEquals(sender, MnuPly1Human) Then Game.Ply(0) = New HumanPlayer
        If Object.ReferenceEquals(sender, MnuPly1Computer) Then Game.Ply(0) = New ComputerPlayer
        If Object.ReferenceEquals(sender, MnuPly1Perfect) Then Game.Ply(0) = New PerfectPlayer
        If Object.ReferenceEquals(sender, MnuPly1Combined) Then Game.Ply(0) = New CombinedPlayer
        If Object.ReferenceEquals(sender, MnuPly2Human) Then Game.Ply(1) = New HumanPlayer
        If Object.ReferenceEquals(sender, MnuPly2Computer) Then Game.Ply(1) = New ComputerPlayer
        If Object.ReferenceEquals(sender, MnuPly2Perfect) Then Game.Ply(1) = New PerfectPlayer
        If Object.ReferenceEquals(sender, MnuPly2Combined) Then Game.Ply(1) = New CombinedPlayer
        Game.UpdateLabels()
    End Sub

    Private Sub Main_FormClosed(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosedEventArgs) Handles Me.FormClosed
        If TypeOf Game.Ply(0) Is UDPPlayer OrElse TypeOf Game.Ply(1) Is UDPPlayer Then UDPPlayer.ReportMatchResult()
    End Sub

    'Public UjEval(16777216) As Integer '2^24
    'Private Sub InitUjEval()
    '    For i = 0 To 16777216
    '        UjEval(i) = 1000000
    '    Next
    '    Dim be As New System.IO.StreamReader("eval.txt")
    '    While Not be.EndOfStream
    '        Dim ln = be.ReadLine().Split()
    '        Dim index As Integer = 0
    '        For i = 1 To 12
    '            Select Case ln(i)
    '                Case 0 : index = index Xor (1 << (i - 1))
    '                Case 1 : index = index Xor (1 << (i - 1 + 12))
    '            End Select
    '        Next
    '        UjEval(index) = ln(13)
    '    End While
    'End Sub

    Private Sub MnuCopyHistory_Click(sender As System.Object, e As System.EventArgs) Handles MnuCopyHistory.Click
        Clipboard.SetText(Game.history.Aggregate("", Function(str, s) str & Game.ClpString(s) & vbCrLf))
    End Sub

    Private Sub MnuTikzCopy_Click(sender As Object, e As EventArgs) Handles MnuTikzCopy.Click
        Dim s As New System.Text.StringBuilder
        s.AppendLine("\begin{tikzpicture}[]")
        s.AppendLine(" \tikzstyle{vertex}=[circle,draw,minimum size=10pt,inner sep=0pt]")
        Const scale = 0.01 '0.009
        'Dim angles = {180, 135, 90, 45, 0, 315, 270, 225, _
        '              135, 135, 45, 45, 315, 315, 225, 225, _
        '              0, 135, 270, 45, 180, 315, 90, 225}
        Dim angles = {180, 135, 90, 45, 0, 315, 270, 225, _
                      135, 45, 45, 135, 315, 225, 225, 315, _
                      0, 45, 270, 135, 180, 225, 90, 315}

        Dim AdvisorSetMoves As List(Of Tuple(Of PerfectPlayer.Move, Wrappers.gui_eval_elem2))
        Dim OkMoves As SortedSet(Of Integer)
        If _Board.Advisor IsNot Nothing And Game.s.phase = 1 And Not Game.s.KLE Then
            AdvisorSetMoves = _Board.Advisor.GetMoveList(Game.s).Select(Function(m) Tuple.Create(m, _Board.Advisor.MoveValue(Game.s, m))).ToList
            OkMoves = New SortedSet(Of Integer)(PerfectPlayer.AllMaxBy(Function(mvp) mvp.Item2, AdvisorSetMoves, Wrappers.gui_eval_elem2.min_value(_Board.Advisor.GetSec(Game.s))).Select(Function(mvp) mvp.Item1.hov))
        End If

        For i = 0 To 23
            Const emptySize = 6
            Const stoneSize = 13
            Dim x = _Board.BoardNodes(i).X * scale, y = -_Board.BoardNodes(i).Y * scale
            Dim pos = "(" & x & "," & y & ")"
            Dim valueStr = ""
            If Game.s.T(i) = -1 Then
                Dim ii = i
                If AdvisorSetMoves IsNot Nothing Then
                    If OkMoves.Contains(i) Then
                        valueStr = "\bf{!}"
                    End If
                    valueStr &= "$" & AdvisorSetMoves.Where(Function(mvp) mvp.Item1.hov = ii).Max(Function(mvp) mvp.Item2).ToString() & "$"
                End If

                'valueStr = New SetKorong(i).ToString

                s.AppendLine(String.Format("  \node[vertex] (S-{0}) at {1} [label={2}:\footnotesize {5}, minimum size={3}pt, fill=black] {4};", i, pos, angles(i), emptySize, "{}", valueStr))
            ElseIf Game.s.T(i) = 0 Then
                If AdvisorSetMoves IsNot Nothing Then
                    valueStr = "-"
                End If
                s.AppendLine(String.Format("  \node[vertex] (S-{0}) at {1} [label={4}:\footnotesize {5}, minimum size={2}pt, line width=0.8pt] {3};", i, pos, stoneSize, "{}", angles(i), valueStr))
            Else
                If AdvisorSetMoves IsNot Nothing Then
                    valueStr = "-"
                End If
                s.AppendLine(String.Format("  \node[vertex] (S-{0}) at {1} [label={4}:\footnotesize {5}, minimum size={2}pt, fill=black] {3};", i, pos, stoneSize, "{}", angles(i), valueStr))
            End If
        Next
        For i = 0 To 23
            For j = 1 To Rules.ALBoardGraph(i, 0)
                s.Append(String.Format("\draw (S-{0}) -- (S-{1});", i, Rules.ALBoardGraph(i, j)))
            Next
        Next
        s.AppendLine()
        s.AppendLine("\end{tikzpicture}")
        Clipboard.SetText(s.ToString())
    End Sub

    Private Sub MnuCopyMoveList_Click(sender As Object, e As EventArgs) Handles MnuCopyMoveList.Click
        Game.CopyMoveList()
    End Sub

    Private Sub AdvisorToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AdvisorToolStripMenuItem.Click
        _Board.SwitchAdvisor()
    End Sub

    Private Sub WebsiteToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles WebsiteToolStripMenuItem.Click
        Process.Start("http://compalg.inf.elte.hu/~ggevay/mills/index.php")
    End Sub
    Private Sub ManualToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ManualToolStripMenuItem.Click
        Process.Start(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory, "Readme.txt"))
    End Sub
    Private Sub AboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AboutToolStripMenuItem.Click
        AboutBox.ShowDialog()
    End Sub
End Class


Public Class Game
    Public frm As FrmMain 'the main form
    Private _Ply(1) As Player 'players in the game
    Public history As New LinkedList(Of GameState) 'GameStates in this (and previous) games
    Private current As LinkedListNode(Of GameState) 'the node of the current GameState in history
    'Private MoveList As New List(Of Move)
    'Private MoveListCurIndex As Integer = 0
    Public ReadOnly Property s As GameState 'wrapper of current.value
        Get
            Return current.Value
        End Get
    End Property

    Public Sub New(ByVal p1 As Player, ByVal p2 As Player, ByVal _frm As FrmMain)
        frm = _frm
        history.AddLast(New GameState)
        current = history.Last
        Ply(0) = p1
        Ply(1) = p2
        frm._Board.UpdateGameState(s)
        UpdateLabels()
    End Sub

    Public Function Plys() As Player()
        Return _Ply
    End Function
    Public Property Ply(ByVal i As Integer) As Player 'get or set players in the game
        Get
            Return _Ply(i)
        End Get
        Set(ByVal p As Player)
            If p Is Nothing Then
                _Ply(i) = Nothing
                Return
            End If

            p.Quit() 'p-t kiléptetjük, hátha esetleg benne volt egy játékban (pl. NewGame-nél az ezelõttiben)
            If _Ply(i) IsNot Nothing Then _Ply(i).Quit() 'kiléptetjük azt a játékost, akinek a helyére p jön
            _Ply(i) = p
            If i = 0 Then 'set menus
                frm.MnuPly1Human.Checked = p.GetType() = GetType(HumanPlayer)
                frm.MnuPly1Computer.Checked = p.GetType() = GetType(ComputerPlayer)
                frm.MnuPly1Perfect.Checked = p.GetType() = GetType(PerfectPlayer)
                frm.MnuPly1Combined.Checked = p.GetType() = GetType(CombinedPlayer)
            Else
                frm.MnuPly2Human.Checked = p.GetType() = GetType(HumanPlayer)
                frm.MnuPly2Computer.Checked = p.GetType() = GetType(ComputerPlayer)
                frm.MnuPly2Perfect.Checked = p.GetType() = GetType(PerfectPlayer)
                frm.MnuPly2Combined.Checked = p.GetType() = GetType(CombinedPlayer)
            End If
            p.Enter(Me)
            NotifyPlayer(i)
        End Set
    End Property

    <System.Runtime.ExceptionServices.HandleProcessCorruptedStateExceptionsAttribute>
    Public Sub MakeMove(ByVal M As Move) 'a player objektumok hívják meg, amikor lépni szeretnének
        Try
            'Debug.Print(Microsoft.VisualBasic.Timer & " MakeMove, sidetomove: " & s.SideToMove) '

            Ply(1 - s.SideToMove).FollowMove(M)

            history.AddAfter(current, New GameState(s))
            current = current.Next

            s.MakeMove(M)

            'MoveList.RemoveRange(MoveListCurIndex, MoveList.Count - MoveListCurIndex)
            'MoveList.Add(M)
            'MoveListCurIndex += 1

            frm.UpdateUI(s)

            If s.over Then
                Ply(0).Over(s)
                Ply(1).Over(s)
            Else
                NotifyPlayers()
            End If
        Catch ex As Exception
            If TypeOf ex Is KeyNotFoundException Then Throw
            MsgBox("Exception in MakeMove" & vbCrLf & ex.ToString)
        End Try
    End Sub

    Delegate Sub DToMove(ByVal s As GameState)
    Private Sub NotifyPlayer(ByVal i As Integer)
        If s.SideToMove = i Then
            frm.BeginInvoke(New DToMove(AddressOf Ply(i).ToMove), s) 'itt azért kell BeginInvoke-ot használni, mert azt szeretnénk, hogy a hívó elvégezhesse a dolgát, mielõtt a játékos értesül róla, hogy lépnie kell
        Else
            frm.BeginInvoke(New DToMove(AddressOf Ply(i).OppToMove), s)
        End If
    End Sub
    Private Sub NotifyPlayers()
        'Debug.Print(Microsoft.VisualBasic.Timer & " NotifyPlayers, sidetomove: " & s.SideToMove) '
        For i = 0 To 1
            NotifyPlayer(i)
        Next
    End Sub
    'Public Sub QuitPlayers()
    '    For i = 0 To 1
    '        Ply(i).Quit()
    '    Next
    'End Sub
    Public Sub CancelThinking()
        For i = 0 To 1
            Ply(i).CancelThinking()
        Next
    End Sub
    Public Sub UpdateLabels()
        frm.StatusGraphics.FillEllipse(If(s.SideToMove = 0, Brushes.White, Brushes.Black), New Rectangle(1, 1, frm.StatusStrip.Height - 2, frm.StatusStrip.Height - 2))

        If Ply(0).GetType = Ply(1).GetType Then
            If Not s.KLE Then frm.LblKov.Text = If(s.SideToMove = 0, "1st", "2nd") & " player to move."
        ElseIf TypeOf Ply(s.SideToMove) Is HumanPlayer Then
            frm.LblKov.Text = "Human to move."
        ElseIf TypeOf Ply(s.SideToMove) Is CombinedPlayer Then 'sorrend!
            frm.LblKov.Text = "Combined to move."
        ElseIf TypeOf Ply(s.SideToMove) Is ComputerPlayer Then
            frm.LblKov.Text = "Computer to move."
        ElseIf TypeOf Ply(s.SideToMove) Is PerfectPlayer Then
            frm.LblKov.Text = "Perfect player to move."
        ElseIf TypeOf Ply(s.SideToMove) Is UDPPlayer Then
            frm.LblKov.Text = "UDP to move."
        Else
            frm.LblKov.Text = "Fld to move."
        End If

        If TypeOf Ply(s.SideToMove) Is HumanPlayer Then
            If s.KLE Then frm.LblKov.Text = "Take a stone."
            If s.phase = 1 Then
                'Dim pr = MaxKSZ - s.FölrakottKorongCount(s.SideToMove)
                'If pr > 0 Then
                '    frm.LblFölrak.Text = pr & " stones to place."
                'Else
                '    frm.LblFölrak.Text = "No more stones to place."
                'End If
                frm.LblSetnum.Text = MaxKSZ - s.SetStoneCount(0) & ", " & MaxKSZ - s.SetStoneCount(1) & " stones to place."
            Else
                frm.LblSetnum.Text = ""
            End If
        Else
            frm.LblSetnum.Text = ""
        End If

        frm.LblLastKle.Text = "Last irreversible move: " & s.LastKLE
    End Sub
    Public Function PlayertypeChangingCmdAllowed() As Boolean
        'Return TypeOf Ply(s.SideToMove) Is HumanPlayer
        Return True
    End Function
    Public Function Undo() As Boolean
        If Not PlayertypeChangingCmdAllowed() Then Return False
        If TypeOf Ply(current.Value.SideToMove) Is ComputerPlayer Then Return False
        frm._Board.ClearMezoSelection()

        If current.Previous Is Nothing Then Return False
        Dim tmp = current
        Do
            current = current.Previous
        Loop While (current.Previous IsNot Nothing AndAlso Not TypeOf Ply(current.Value.SideToMove) Is HumanPlayer) 'addig vonunk vissza, hogy ne gép következzen
        If TypeOf Ply(current.Value.SideToMove) Is ComputerPlayer Then 'ha nem sikerült, akkor visszaállítjuk az eredeti állapotot
            current = tmp
            Return False
        Else
            'MoveListCurIndex -= 1

            frm.UpdateUI(s)
            NotifyPlayers() '
            Return True
        End If
    End Function
    Public Function Redo() As Boolean
        'If MoveListCurIndex = MoveList.Count Then Return False 'hack arra, hogy ne lehessen a regebbi agba atkerulni, hogy ne romolhasson el a MoveList konzisztenciaja

        If Not PlayertypeChangingCmdAllowed() Then Return False
        If TypeOf Ply(current.Value.SideToMove) Is ComputerPlayer Then Return False
        frm._Board.ClearMezoSelection()

        If current.Next Is Nothing Then Return False
        Dim tmp = current
        Do
            current = current.Next
        Loop While (current.Next IsNot Nothing AndAlso Not TypeOf Ply(current.Value.SideToMove) Is HumanPlayer) 'addig megyünk elõre, hogy ne gép következzen
        If TypeOf Ply(current.Value.SideToMove) Is ComputerPlayer Then 'ha nem sikerült, akkor visszaállítjuk az eredeti állapotot
            current = tmp
            Return False
        Else
            'MoveListCurIndex += 1

            frm.UpdateUI(s)
            NotifyPlayers() '
            Return True
        End If
    End Function

    Public Function ClpString(s As GameState) As String
        Return s.ToString() & "," & If(TypeOf Ply(0) Is CombinedPlayer, 4, If(TypeOf Ply(0) Is ComputerPlayer, 2, If(TypeOf Ply(0) Is PerfectPlayer, 3, 0))) & "," & If(TypeOf Ply(1) Is CombinedPlayer, 4, If(TypeOf Ply(1) Is ComputerPlayer, 2, If(TypeOf Ply(1) Is PerfectPlayer, 3, 0))) & ",malom2" 'sorrend az ifekben!
    End Function
    Public Sub copy()
        'Clipboard.SetText(s.ToString() & "," & If(TypeOf Ply(0) Is ComputerPlayer, 2, 0) & "," & If(TypeOf Ply(1) Is ComputerPlayer, 2, 0) & ",malom2")
        Clipboard.SetText(ClpString(s))
    End Sub
    Public Sub paste()
        If Not PlayertypeChangingCmdAllowed() Then Return
        CancelThinking()
        Dim clpml As String = Clipboard.GetText()
        Dim clplines = clpml.Split(New String() {vbCrLf}, StringSplitOptions.RemoveEmptyEntries)
        Dim success_count = 0
        Dim first = True
        For Each clp In clplines
            If Int64.TryParse(clp, vbNull) Then clp = Wrappers.Helpers.toclp(Int64.Parse(clp))
            Dim ss() As String = clp.Split(",")
            Try
                Try
                    Try
                        Dim newGameState = New GameState(clp)
                        history.AddAfter(current, newGameState)
                        current = current.Next
                        success_count += 1
                        If success_count = clplines.Length Then
                            Ply(0) = If(ss(35) = 4, New CombinedPlayer, If(ss(35) = 2, New ComputerPlayer, If(ss(35) = 3, New PerfectPlayer, New HumanPlayer)))
                            Ply(1) = If(ss(36) = 4, New CombinedPlayer, If(ss(36) = 2, New ComputerPlayer, If(ss(36) = 3, New PerfectPlayer, New HumanPlayer)))
                        End If
                    Catch ex As FormatException
                        If clplines.Count = 1 Then
                            frm.LblSetnum.Text = frm.LblSetnum.Text & " Not game state on clipboard."
                            Return
                        End If
                    Catch ex As InvalidGameStateException
                        If clplines.Count = 1 Then
                            frm.LblSetnum.Text = frm.LblSetnum.Text & ex.mymsg
                            Return
                        End If
                    Catch ex As IndexOutOfRangeException
                        Ply(0) = If(ss(25) = 2, New ComputerPlayer, If(ss(25) = 3, New PerfectPlayer, New HumanPlayer))
                        Ply(1) = If(ss(26) = 2, New ComputerPlayer, If(ss(26) = 3, New PerfectPlayer, New HumanPlayer))
                        success_count += 1
                    End Try
                Catch ex As InvalidCastException When ss(35) = "malom"
                    Ply(0) = If(ss(25) = 2, New ComputerPlayer, If(ss(25) = 3, New PerfectPlayer, New HumanPlayer))
                    Ply(1) = If(ss(26) = 2, New ComputerPlayer, If(ss(26) = 3, New PerfectPlayer, New HumanPlayer))
                    success_count += 1
                End Try
            Catch ex As InvalidCastException
                If clplines.Count = 1 Then
                    frm.LblSetnum.Text = frm.LblSetnum.Text & " Not game state on clipboard."
                    Return
                End If
            End Try
        Next
        frm.UpdateUI(s)
        If clplines.Count > 1 Then frm.LblSetnum.Text = frm.LblSetnum.Text & " " & success_count & " game states pasted."
    End Sub
    Public Sub SwitchPlayers()
        If TypeOf Ply(0) Is ComputerPlayer And TypeOf Ply(1) Is ComputerPlayer Then Return

        CancelThinking()

        Dim p0 = Ply(0)
        Dim p1 = Ply(1)
        Ply(0) = Nothing
        Ply(1) = Nothing
        Ply(0) = p1
        Ply(1) = p0

        'Dim tmp = Ply(0)
        'Ply(0) = Ply(1)
        'Ply(1) = tmp

        UpdateLabels()
    End Sub

    Public Sub CopyMoveList()
        Throw New NotImplementedException

        'this is buggy with undo

        'Dim s = ""
        'For i = 0 To MoveListCurIndex - 1
        '    s &= MoveList(i).ToString
        '    If i < MoveListCurIndex - 1 AndAlso Not TypeOf MoveList(i + 1) Is LeveszKorong Then s &= ", "
        'Next
        'Clipboard.SetText(s)
    End Sub
End Class

Public Class GameState
    Public T(23) As Integer 'a tábla (-1: üres, 0: fehér korong, 1: fekete korong)
    Public phase As Integer = 1
    Public SetStoneCount(1) As Integer 'how many stones the players have set
    Public StoneCount(1) As Integer
    Public KLE As Boolean 'koronglevétel jön-e
    Public SideToMove As Integer
    Public MoveCount As Integer
    Public over As Boolean
    Public winner As Integer '(-1, ha döntetlen)
    Public block As Boolean
    Public LastKLE As Integer

    Public Sub New() 'játszma eleje
        For i = 0 To 23
            T(i) = -1
        Next
    End Sub
    Public Sub New(ByVal s As GameState)
        T = s.T.ToArray 'deep copy 
        phase = s.phase
        SetStoneCount = s.SetStoneCount.ToArray
        StoneCount = s.StoneCount.ToArray
        KLE = s.KLE
        SideToMove = s.SideToMove
        MoveCount = s.MoveCount
        over = s.over
        winner = s.winner
        block = s.block
        LastKLE = s.LastKLE
    End Sub

    Public Sub MakeMove(ByVal M As Object)
        If Not TypeOf (M) Is Move Then Throw New ArgumentException()

        MoveCount += 1
        If TypeOf M Is SetKorong Then
            T(M.hov) = SideToMove
            SetStoneCount(SideToMove) += 1
            StoneCount(SideToMove) += 1
            LastKLE = 0
        ElseIf TypeOf M Is MoveKorong Then
            T(M.hon) = -1
            T(M.hov) = SideToMove
            LastKLE += 1
            If LastKLE >= LastKLELimit Then
                over = True
                winner = -1 'draw
            End If
        ElseIf TypeOf M Is LeveszKorong Then
            T(M.hon) = -1
            StoneCount(1 - SideToMove) -= 1
            KLE = False
            'If szakasz = 2 And KorongCount(1 - SideToMove) = 2 Then
            If StoneCount(1 - SideToMove) + MaxKSZ - SetStoneCount(1 - SideToMove) < 3 Then
                over = True
                winner = SideToMove
            End If
            LastKLE = 0
        End If
        If (TypeOf M Is SetKorong Or TypeOf M Is MoveKorong) AndAlso Malome(M.hov, Me) > -1 Then 'ha malmot csinált a lépés
            KLE = True
        Else
            SideToMove = 1 - SideToMove
            If SetStoneCount(0) = MaxKSZ And SetStoneCount(1) = MaxKSZ And phase = 1 Then phase = 2 'korongmozgatásra váltás
            If Not TudLépni(Me) Then
                over = True
                block = True
                winner = 1 - SideToMove
                If Wrappers.Constants.FBD AndAlso StoneCount(0) = 12 AndAlso StoneCount(1) = 12 Then
                    winner = -1
                End If
            End If
        End If
    End Sub

    Public Sub New(ByVal s As String) 'vágólapról beillesztéshez
        Dim ss() As String = s.Split(",")
        Try
            If ss(33) = "malom" OrElse ss(34) = "malom" OrElse ss(35) = "malom" OrElse ss(37) = "malom2" Then 'tudni kell értelmezni a régebbi formátumokat is
                For i = 0 To 23
                    T(i) = ss(i)
                Next
                SideToMove = ss(24)
                phase = ss(27)
                SetStoneCount(0) = ss(28)
                SetStoneCount(1) = ss(29)
                StoneCount(0) = ss(30)
                StoneCount(1) = ss(31)
                KLE = ss(32)
                If ss(33) <> "malom" Then MoveCount = ss(33) Else MoveCount = 10 'csak azért 10, hogy ne 0 legyen, mert akkor nem gondolkodna a következõ két lépésnél, mert azt hinné, hogy a játék eleje van
                If ss(33) <> "malom" AndAlso ss(34) <> "malom" Then LastKLE = ss(34) Else LastKLE = 0
                If StoneCount(0) <> T.Count(Function(x) x = 0) Or StoneCount(1) <> T.Count(Function(x) x = 1) Then Throw New InvalidGameStateException(" Number of stones is incorrect.")
            Else
                Throw New FormatException
            End If
        Catch ex As InvalidGameStateException
            Throw ex
        Catch ex As Exception
            Throw New FormatException
        End Try
    End Sub

    Public Overrides Function ToString() As String 'for clipboard
        Dim s As New System.IO.StringWriter
        For i = 0 To 23
            s.Write(T(i) & ",")
        Next
        s.Write(SideToMove & "," & 0 & "," & 0 & "," & phase & "," & SetStoneCount(0) & "," & SetStoneCount(1) & "," & StoneCount(0) & "," & StoneCount(1) & "," & KLE & "," & MoveCount & "," & LastKLE)
        Return s.ToString()
    End Function
End Class

Class InvalidGameStateException
    Inherits Exception
    Public mymsg As String
    Public Sub New(ByVal msg As String)
        Me.mymsg = msg
    End Sub
End Class