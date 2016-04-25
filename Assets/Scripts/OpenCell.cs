using UnityEngine;
using System.Collections;

public struct CellParams {
	public bool Top;
	public bool Bottom;
	public bool LeftTop;
	public bool LeftBottom;
	public bool RightTop;
	public bool RightBottom;

	public void uncheckAll(){
		Top = Bottom = LeftBottom = LeftTop = RightTop = RightBottom = false;
	}

	public bool haveBlockAdj(){
		return (Top | Bottom | LeftBottom | LeftTop | RightTop | RightBottom); 
	}
}

public class OpenCell : MonoBehaviour {
	public int cellValue;
	public bool isMine;
	public bool isFlag;

	public bool isUp;
	public bool isOpen = false;

	public CellParams cellParams; 

	public Vector2 cellCoord;
	public FieldBlock parentBlock;
	
	private bool mouseDowned = false;
	private bool cancelOpen = false;
	
	private GameObject mainCamera;

	private int needcells;
	private float tapTime = 0;
	private float flagTime = 0.75f;

	private bool isFlagSet = false;

	public Material FlagMaterial;
	public Material CellMaterial;

	private UIProcs uiProcs;
	private FieldProcs fieldProcs;
	private FieldMover fieldMover;

	void Start(){
		GameObject gameController = GameObject.FindGameObjectWithTag ("GameController");
		mainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		fieldMover = gameController.GetComponent<FieldMover> ();
		uiProcs = gameController.GetComponent<UIProcs> ();
		fieldProcs = GameObject.FindGameObjectWithTag ("Field").GetComponent<FieldProcs> ();

		needcells = fieldProcs.GetFieldSize() * fieldProcs.GetFieldSize() * 6 - fieldProcs.GetMinesCount();
	}

	void CheckAdjFieldBlock(Vector2 adjBlockPos){
		FieldBlock adjBlock = fieldProcs.FindFieldBlock(adjBlockPos);
		if (adjBlock == null){
			adjBlock = fieldProcs.AddFieldBlock(adjBlockPos, true);
			fieldProcs.SetMinesOnFieldBlock(adjBlock, null, true);
			fieldProcs.CalcValuesInFieldBlock(adjBlock);
		}
	}

	void Open(){
		if (fieldMover == null)
			Start ();

		bool isGameOver = fieldMover.GetGameOver ();
		bool isWin = fieldMover.GetWin ();
		if (isGameOver || isWin || isFlag) return;

		if (!fieldMover.IsFirstOpen ()) {
			fieldProcs.SetMinesOnFieldBlock(parentBlock, transform.parent.gameObject, uiProcs.IsEndlessGame());
			fieldProcs.CalcValuesInFieldBlock(parentBlock);
			fieldMover.FirstOpen();
		}
		if (cellParams.haveBlockAdj ()) {
			Vector2 pos = parentBlock.pos;
			if (cellParams.Top) {
				Vector2 adjPos = new Vector2(pos.x, pos.y + 1);
				CheckAdjFieldBlock(adjPos);
			}
			if (cellParams.Bottom) {
				Vector2 adjPos = new Vector2(pos.x, pos.y - 1);
				CheckAdjFieldBlock(adjPos);
			}
			if (cellParams.LeftTop) {
				Vector2 adjPos = new Vector2(pos.x - 1, pos.y + 1);
				CheckAdjFieldBlock(adjPos);
			}
			if (cellParams.LeftBottom) {
				Vector2 adjPos = new Vector2(pos.x - 1, pos.y);
				CheckAdjFieldBlock(adjPos);
			}
			if (cellParams.RightTop) {
				Vector2 adjPos = new Vector2(pos.x + 1, pos.y);
				CheckAdjFieldBlock(adjPos);
			}
			if (cellParams.RightBottom) {
				Vector2 adjPos = new Vector2(pos.x + 1, pos.y - 1);
				CheckAdjFieldBlock(adjPos);
			}
			fieldProcs.CalcValuesInFieldBlock(parentBlock);
		}

		isOpen = true;
		if ((cellValue == 0) && (!isMine)) {
			for (int i = 0; i < fieldProcs.adjOffsets.Length; i++) {
				OpenCell adjOpenCell = fieldProcs.GetAdjOpenCell(this, i); 
				if ((adjOpenCell == null) || (adjOpenCell.isOpen)) continue;
				if (adjOpenCell.isFlag) {
					adjOpenCell.isFlag = false;
					uiProcs.ChangeFlagsCount(1);
				}
				adjOpenCell.Open ();
			}
		}

		if (isMine) {
			if (uiProcs.IsEndlessGame ())
				fieldMover.SaveScore ();
			fieldMover.SetGameOver (true);
			fieldProcs.ViewMines ();
		} else {
			uiProcs.IncOpenedCells();
		}
		if ((!uiProcs.IsEndlessGame()) && (fieldMover.openedCells >= needcells)) {
			fieldMover.SaveScore ();
			fieldMover.SetWin (true);
		}

		gameObject.SetActive (false);
	}

	void SetFlag(){
		bool flag_set = false;
		if (isFlag) {
			if (!uiProcs.IsEndlessGame())
				flag_set = uiProcs.ChangeFlagsCount(1);
			else 
				flag_set = uiProcs.ChangeFlagsCount(-1);
			if (flag_set) GetComponent<Renderer>().material = CellMaterial;
		} else {
			if (!uiProcs.IsEndlessGame())
				flag_set = uiProcs.ChangeFlagsCount(-1);
			else 
				flag_set = uiProcs.ChangeFlagsCount(1);
			if (flag_set) GetComponent<Renderer>().material = FlagMaterial;
		}
		if (flag_set)
		  isFlag = !isFlag;
	}

	public void CancelOpen(){
		cancelOpen = true;
		uiProcs.HideWaiter();
	}

	void OnMouseDown () {
		if (fieldMover.IsPaused() || fieldMover.GetGameOver()) return;
		GameObject currCell = transform.parent.gameObject;
		fieldProcs.SetCurrentCell (currCell);
		mouseDowned = true;
		tapTime = 0;
		isFlagSet = false;

		Vector3 pickpos = mainCamera.GetComponent<Camera>().ScreenToWorldPoint (Input.mousePosition);
		uiProcs.ShowWaiter(pickpos.x, pickpos.z, flagTime);
	}

	void OnMouseUp (){
		if (fieldMover.IsPaused() || fieldMover.GetGameOver()) return;
		if (uiProcs.isWaiterActive ()) 
			CancelOpen ();
		if (mouseDowned)
			if (!cancelOpen){
				if (tapTime < flagTime)
					Open ();
			}
		mouseDowned = false;
		cancelOpen = false;
		tapTime = 0;
		uiProcs.HideWaiter();
	}

	void FixedUpdate(){
		if (mouseDowned)
			tapTime += Time.deltaTime;

		if ((tapTime > flagTime) && (!isFlagSet) && (!cancelOpen)) {
			SetFlag ();
			isFlagSet = true;
		}
	}
}
