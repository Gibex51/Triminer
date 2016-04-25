using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldBlock {
	public Vector2 pos;
	public GameObject block;
	public GameObject[] cells;
	public OpenCell[] cups;
	public GameObject opto_field;
	public GameObject opto_mines;
}

public class FieldProcs : MonoBehaviour {
	public GameObject Cell;
	public GameObject CellText;
	public GameObject Mine;

	public int currLevel;
	public int[] fieldSizeLevels = {3, 5, 7};
	public int[] fieldMinesLevels = {6, 27, 64};

	int fieldSize;
	int minesOnField;
	
	public float triHeight;
	public float triSideLen;

	private GameObject currentCell = null;

	private bool isGameStarted = false;
	private bool gameOver;
	
	private Quaternion upRotate = Quaternion.Euler (270, 180, 0);
	private Quaternion downRotate = Quaternion.Euler (270, 0, 0);
	
	public Vector2[] adjOffsets = null;
	
	//-------------------------------------------------------------

	Dictionary <int, FieldBlock> blocks;
	Dictionary <int, GameObject> allCells;

	private int BlockCoordToID(Vector2 blockCoord) {
		int x = Mathf.RoundToInt (blockCoord.x);
		int y = Mathf.RoundToInt (blockCoord.y);
		if (x > 500) x = 500;
		if (y > 500) y = 500;
		if (x < -500) x = -500;
		if (y < -500) y = -500;
		return (x + 500) * 1000 + (y + 500);
	}

	private int CellsCoordToID(Vector2 cellCoord) {
		int x = Mathf.RoundToInt (cellCoord.x);
		int y = Mathf.RoundToInt (cellCoord.y);
		if (x > 5000) x = 5000;
		if (y > 5000) y = 5000;
		if (x < -5000) x = -5000;
		if (y < -5000) y = -5000;
		return (x + 5000) * 10000 + (y + 5000);
	}

	private Vector2 GetCentralCellCoordInBlock(Vector2 blockCoord) {
		int x = Mathf.RoundToInt (blockCoord.x);
		int y = Mathf.RoundToInt (blockCoord.y);
		y = y * fieldSize * 2 + x * fieldSize;
		x = x * fieldSize * 3;
		return new Vector2(x, y);
	}

	private Vector3 BlockCoordToRealCoord(Vector2 pos) {
		float fieldWidth = triSideLen * fieldSize * 2;
		float fieldHeight = triHeight * fieldSize * 2;
		return new Vector3 (
						pos.x * (fieldWidth * 0.75f), 
						0,
						pos.x * (fieldHeight * 0.5f) + pos.y * fieldHeight);
	}

	//-------------------------------------------------------------

	public void SetCurrentCell( GameObject currCell ){
		currentCell = currCell;
	}

	public GameObject GetCurrentCell(){
		return currentCell;
	}

	void Start () {
		blocks = new Dictionary <int, FieldBlock> ();
		allCells = new Dictionary <int, GameObject> ();

		adjOffsets = new Vector2[12];
		adjOffsets [0] = new Vector2 (-2, 1);
		adjOffsets [1] = new Vector2 (-1, 1);
		adjOffsets [2] = new Vector2 (0, 1);
		adjOffsets [3] = new Vector2 (1, 1);
		adjOffsets [4] = new Vector2 (2, 1);
		adjOffsets [5] = new Vector2 (-2, 0);
		adjOffsets [6] = new Vector2 (-1, 0);
		adjOffsets [7] = new Vector2 (1, 0);
		adjOffsets [8] = new Vector2 (2, 0);
		adjOffsets [9] = new Vector2 (-1, -1);
		adjOffsets [10] = new Vector2 (0, -1);
		adjOffsets [11] = new Vector2 (1, -1);
		
		Application.targetFrameRate = 40;
		isGameStarted = false;
	}

	public bool GetGameStarted(){
		return isGameStarted;
	}

	public void StartGame(){
		isGameStarted = true;
	}

	public int GetFieldSize(){
		return fieldSize;
	}

	public int GetMinesCount(){
		return minesOnField;
	}

	public void SetLevel(int level){
		if ((level > 0) && (level <= fieldSizeLevels.Length)) {
			fieldSize = fieldSizeLevels [level - 1];
			minesOnField = fieldMinesLevels [level - 1];
			currLevel = level;
		} else {
			fieldSize = fieldSizeLevels [0];
			minesOnField = fieldMinesLevels [0];
			currLevel = 0;
		}
	}

	public void SetCustomLevel(int RadiusOfField, int MinesPercent){
		if (RadiusOfField < 2) RadiusOfField = 2;
		if (RadiusOfField > 9) RadiusOfField = 9;
		if (MinesPercent < 5) MinesPercent = 5;
		if (MinesPercent > 60) MinesPercent = 60;
		fieldSize = RadiusOfField;
		minesOnField = Mathf.RoundToInt((RadiusOfField * RadiusOfField * 6 * MinesPercent) / 100.0f);
		currLevel = -1;
	}

	public void ViewMines(){
		foreach (int curr_cell in allCells.Keys) {
			Transform cell_cup = allCells[curr_cell].transform.Find("CellCup");
			if (cell_cup != null) {
				OpenCell oCell = cell_cup.gameObject.GetComponent<OpenCell>();
				if (oCell.isMine) {
					Color col = cell_cup.gameObject.GetComponent<Renderer>().material.color;
					col.a = 0.6f;
					cell_cup.gameObject.GetComponent<Renderer>().material.color = col;
				}
			}
		}
	}

	void FieldBlock_OptimizeBack (FieldBlock fBlock) {
		int ncells = fBlock.cells.Length;

		CombineInstance[] combo_field = new CombineInstance[ncells];

		Material shared_mat_field = fBlock.cells[0].transform.Find("Cell").gameObject.GetComponent<Renderer>().sharedMaterial;

		for (int curr_cell = 0; curr_cell < ncells; curr_cell++) {
			GameObject cell = fBlock.cells[curr_cell].transform.Find("Cell").gameObject;
			MeshFilter cell_mf = cell.GetComponent<MeshFilter>();
			combo_field[curr_cell].mesh = cell_mf.sharedMesh;
			combo_field[curr_cell].transform = cell_mf.transform.localToWorldMatrix;
			DestroyImmediate (cell);
		}

		fBlock.opto_field = new GameObject();
		fBlock.opto_field.name = "OptimizedField";
		fBlock.opto_field.AddComponent<MeshFilter> ();
		fBlock.opto_field.AddComponent<MeshRenderer> ();                                                                                                                                                                                                                                                                                                                                                          
		fBlock.opto_field.GetComponent<MeshFilter> ().mesh.CombineMeshes (combo_field);
		fBlock.opto_field.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		fBlock.opto_field.GetComponent<MeshRenderer> ().receiveShadows = false;
		fBlock.opto_field.GetComponent<Renderer>().sharedMaterial = shared_mat_field;
		fBlock.opto_field.transform.parent = transform;

		combo_field = null;
	}

	void FieldBlock_OptimizeNumbersAndMines(FieldBlock fBlock){
		int ncells = fBlock.cells.Length;
		int nums_mines_Count = 0;
		
		for (int curr_cell = 0; curr_cell < ncells; curr_cell++) {
			OpenCell oCell = fBlock.cups[curr_cell];
			if ((oCell.isMine) || (oCell.cellValue > 0))
				nums_mines_Count ++;
		}
		
		CombineInstance[] combo_nums_mines = new CombineInstance[nums_mines_Count];
		Material shared_mat_num_mine = null;
		int curr_num_mine = 0;
		
		for (int curr_cell = 0; curr_cell < ncells; curr_cell++) {
			Transform cMine = fBlock.cells[curr_cell].transform.Find("Mine(Clone)");
			Transform cNum = fBlock.cells[curr_cell].transform.Find("CellText(Clone)");
			GameObject num_mine = null;
			if (cMine != null){
				num_mine = cMine.gameObject;
			}
			if (cNum != null) {
				num_mine = cNum.gameObject;
			}

			if (num_mine != null) {
				MeshFilter num_mf = num_mine.GetComponent<MeshFilter>();
				
				combo_nums_mines[curr_num_mine].mesh = num_mf.mesh;
				combo_nums_mines[curr_num_mine].transform = num_mf.transform.localToWorldMatrix;
				
				Vector3[] numb_vertices  = combo_nums_mines[curr_num_mine].mesh.vertices;
				Vector2[] numb_uvs = new Vector2[numb_vertices.Length];
				
				Vector2 numb_offset = num_mine.GetComponent<Renderer>().sharedMaterial.GetTextureOffset("_MainTex");
				Vector2 numb_scale = num_mine.GetComponent<Renderer>().sharedMaterial.GetTextureScale("_MainTex");
				
				numb_uvs[0] = new Vector2(numb_offset.x, numb_offset.y);
				numb_uvs[1] = new Vector2(numb_offset.x + numb_scale.x, numb_offset.y + numb_scale.y);
				numb_uvs[2] = new Vector2(numb_offset.x + numb_scale.x, numb_offset.y);
				numb_uvs[3] = new Vector2(numb_offset.x, numb_offset.y + numb_scale.y);
				
				combo_nums_mines[curr_num_mine].mesh.uv = numb_uvs;
				shared_mat_num_mine = num_mine.GetComponent<Renderer>().material;
				DestroyImmediate (num_mine);
				curr_num_mine++;
			}
		}

		if (fBlock.opto_mines != null) {
			DestroyImmediate (fBlock.opto_mines.GetComponent<MeshFilter> ().mesh);
			DestroyImmediate (fBlock.opto_mines);
		}
		fBlock.opto_mines = new GameObject ();
		fBlock.opto_mines.name = "OptimizedNumsAndMines";
		fBlock.opto_mines.AddComponent<MeshFilter> ();
		fBlock.opto_mines.AddComponent<MeshRenderer> (); 
		fBlock.opto_mines.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		fBlock.opto_mines.GetComponent<MeshRenderer> ().receiveShadows = false;
		shared_mat_num_mine.SetTextureOffset ("_MainTex", new Vector2 (0, 0));
		shared_mat_num_mine.SetTextureScale ("_MainTex", new Vector2 (1, 1));
		fBlock.opto_mines.GetComponent<Renderer> ().sharedMaterial = shared_mat_num_mine;                                                                                                                                                                                                                                                                                                                                       
		fBlock.opto_mines.GetComponent<MeshFilter> ().mesh.CombineMeshes (combo_nums_mines);
		fBlock.opto_mines.transform.parent = transform;

		combo_nums_mines = null;
	}

	public OpenCell GetAdjOpenCell(OpenCell srcCell, int adjIndex){
		int upMod = 1;
		if (!srcCell.isUp) upMod = -1;
		Vector2 adjPos = new Vector2(srcCell.cellCoord.x + adjOffsets[adjIndex].x, 
		                             srcCell.cellCoord.y + adjOffsets[adjIndex].y*upMod);
		int adjCellInd = CellsCoordToID(adjPos);
		if (!allCells.ContainsKey (adjCellInd)) 
			return null;

		Transform cellCup = allCells[adjCellInd].transform.Find ("CellCup");
		if (cellCup == null) 
			return null;

		return cellCup.gameObject.GetComponent<OpenCell> ();
	}

	public void SetMinesOnFieldBlock (FieldBlock fieldBlock, GameObject exludeCell, bool Randomize) {
		int ncells = fieldSize * fieldSize * 6;
		// Set Mines
		int randMinesOnField = minesOnField;
		if (Randomize)
			randMinesOnField = Random.Range (minesOnField - 1, minesOnField + 2);

		if (randMinesOnField < 1) randMinesOnField = 1;
		if (randMinesOnField > ncells - 1) randMinesOnField = ncells - 1;
	
		for (int currMine = 0; currMine < randMinesOnField; currMine++)
		while (true){
			int mineCell = Random.Range(0, ncells - 1);
			OpenCell oCell = fieldBlock.cups[mineCell];
			if ((!oCell.isMine) && (fieldBlock.cells[mineCell] != exludeCell)) {
				oCell.isMine = true;
				break;
			}
		}
	}

	public void CalcValuesInFieldBlock(FieldBlock fieldBlock){
		int ncells = fieldSize * fieldSize * 6;

		for (int indCell = 0; indCell < ncells; indCell++) {
			OpenCell oCell = fieldBlock.cups[indCell];

			//Clear cell
			Transform cellText = fieldBlock.cells[indCell].transform.Find("CellText(Clone)");
			Transform cellMine = fieldBlock.cells[indCell].transform.Find("Mine(Clone)");
			if (cellText != null) DestroyImmediate (cellText.gameObject);
			if (cellMine != null) DestroyImmediate (cellMine.gameObject);

			int nMinesInAdj = 0;
			
			for (int indAdj = 0; indAdj < adjOffsets.Length; indAdj++){
				OpenCell adjCell = GetAdjOpenCell(oCell, indAdj);
				if ((adjCell != null) && (adjCell.isMine))
					nMinesInAdj++;
			}
			oCell.cellValue = nMinesInAdj;
			
			Vector3 textOffset = new Vector3(0,0.3f,0);
			if (oCell.isUp) textOffset.z = 0.5f;
			else textOffset.z = -0.5f;
			
			if ((nMinesInAdj > 0) && (!oCell.isMine)){
				GameObject textCell = Instantiate (CellText, fieldBlock.cells[indCell].transform.position + textOffset, 
				                                   CellText.transform.rotation) as GameObject;
				textCell.transform.parent = fieldBlock.cells[indCell].transform; //transform;
				textCell.GetComponent<Renderer>().material.SetTextureOffset("_MainTex", MakeTextureOffset(oCell.cellValue));
			}
			if (oCell.isMine) {
				GameObject mineCell = Instantiate (Mine, fieldBlock.cells[indCell].transform.position + textOffset, 
				                                   Mine.transform.rotation) as GameObject;
				mineCell.transform.parent = fieldBlock.cells[indCell].transform;//transform;
			}
		}

		FieldBlock_OptimizeNumbersAndMines (fieldBlock);
		Resources.UnloadUnusedAssets ();
	}

	Vector2 MakeTextureOffset(int cellValue){
		Vector2 offset = new Vector2 (0.75f,0.75f);
		if ((cellValue >= 1) && (cellValue <= 12)) {
			offset.x = ((cellValue - 1) % 4) * 0.25f;
			offset.y = 0.75f - Mathf.Floor ((cellValue - 1) / 4) * 0.25f;
		}
		return offset;
	}

	void SetCellParams (bool isUp, int col, int colInRow, int row, int cellIndex, bool IsDownIndex, FieldBlock fBlock, bool SetCellType) {
		OpenCell oCell = fBlock.cells[cellIndex].transform.Find("CellCup").gameObject.GetComponent<OpenCell>();
		fBlock.cups [cellIndex] = oCell;
		oCell.isUp = IsDownIndex ? !isUp : isUp;
		oCell.parentBlock = fBlock;

		Vector2 centralCellCoord = GetCentralCellCoordInBlock (fBlock.pos);
		int rowx = Mathf.RoundToInt(centralCellCoord.x)-fieldSize-row; 
		int rowy = 0;
		if (IsDownIndex)
			rowy = Mathf.RoundToInt(centralCellCoord.y)-fieldSize+row;
		else
			rowy = Mathf.RoundToInt(centralCellCoord.y)+fieldSize-row-1;
		oCell.cellCoord = new Vector2(rowx + col, rowy);
		allCells.Add(CellsCoordToID(oCell.cellCoord), fBlock.cells[cellIndex]);

		// Set Cell Type
		oCell.cellParams.uncheckAll ();

		if (SetCellType) {
			if (row == 0) {
				if (IsDownIndex) 
					oCell.cellParams.Bottom = true;
				else
					oCell.cellParams.Top = true;
			}
			if (col <= 1){
				if (IsDownIndex) {
					oCell.cellParams.LeftBottom = true;
					if ((col == 0) && (row == fieldSize - 1)) oCell.cellParams.LeftTop = true;
				}else{
					oCell.cellParams.LeftTop = true;
					if ((col == 0) && (row == fieldSize - 1)) oCell.cellParams.LeftBottom = true;
				}
			}
			if (col >= colInRow - 2){
				if (IsDownIndex) {
					oCell.cellParams.RightBottom = true;
					if ((col == colInRow - 1) && (row == fieldSize - 1)) oCell.cellParams.RightTop = true;
				}else{
					oCell.cellParams.RightTop = true;
					if ((col == colInRow - 1) && (row == fieldSize - 1)) oCell.cellParams.RightBottom = true;
				}
			}
		}
	}

	public void BuildField(bool isEndless) {
		ClearBlocks ();
		ResetFieldPosition ();
		AddFieldBlock (new Vector2 (0, 0), isEndless);
	}

	private Vector3 ResetFieldPosition(){
		Vector3 posOffset = new Vector3 (0, 0, 0);
		transform.position = posOffset;
		return posOffset;
	}

	public FieldBlock FindFieldBlock(Vector2 coord){
		int block_id = BlockCoordToID (coord);
		if (!blocks.ContainsKey (block_id))
			return null;
		return blocks [block_id];
	} 

	public void ClearBlocks(){
		currentCell = null;
		allCells.Clear();
		foreach (int j in blocks.Keys) {
			if (blocks [j].cells != null)
				for (int cellInd = 0; cellInd < blocks[j].cells.Length; cellInd++)
					DestroyImmediate (blocks [j].cells [cellInd]);
			DestroyImmediate (blocks [j].block);

			if (blocks [j].opto_field != null) {
				DestroyImmediate (blocks[j].opto_field.GetComponent<MeshFilter> ().mesh);
				DestroyImmediate (blocks[j].opto_field);
			}
			if (blocks [j].opto_mines != null) {
				DestroyImmediate (blocks[j].opto_mines.GetComponent<MeshFilter> ().mesh);
				DestroyImmediate (blocks[j].opto_mines);
			}
		}
		blocks.Clear();
	}

	private int CellColRowToIndex(int col, int row, bool isDownRow){
		int result = (2 * fieldSize + row) * row + col;
		if (isDownRow) {
			int colInRow = 1 + 2 * (fieldSize + row);
			int cellsCount = fieldSize * fieldSize * 6;
			result = cellsCount - colInRow - result + col * 2;
		}
		return result;
	}
	
	public FieldBlock AddFieldBlock(Vector2 pos, bool endlessBlock){
		FieldBlock newBlock = new FieldBlock();
		Vector3 backupFieldPos = transform.position;
		transform.position = new Vector3 (0, 0, 0);
		newBlock.pos = pos;
		newBlock.block = new GameObject ();
		newBlock.block.name = "Block " + pos.ToString ();
		newBlock.block.transform.parent = transform;
		int cellsCount = fieldSize * fieldSize * 6;
		Vector3 posOffset = new Vector3(0,0,0);
		newBlock.cells = new GameObject[cellsCount];
		newBlock.cups = new OpenCell[cellsCount];

		int cellIndex = 0;
		for (int row = 0; row < fieldSize; row++) {
			int colInRow = 1 + 2 * (fieldSize + row);

			posOffset.x = - triSideLen / 2 * (fieldSize + row + 1);
			posOffset.z = triHeight * (fieldSize - row - 0.5f );
			bool isUp = false;

			for (int col = 0; col < colInRow; col++) {
				posOffset.x += triSideLen / 2;

				if (isUp)
					newBlock.cells[cellIndex] = Instantiate (Cell, posOffset, upRotate) as GameObject;
				else
					newBlock.cells[cellIndex] = Instantiate (Cell, posOffset, downRotate) as GameObject;
				newBlock.cells[cellIndex].transform.parent = newBlock.block.transform;
				SetCellParams(isUp, col, colInRow, row, cellIndex, false, newBlock, endlessBlock);
				posOffset.z = - posOffset.z;

				int cellIndexDown = cellsCount - colInRow - cellIndex + col * 2;

				if (!isUp)
					newBlock.cells[cellIndexDown] = Instantiate (Cell, posOffset, upRotate) as GameObject;
				else
					newBlock.cells[cellIndexDown] = Instantiate (Cell, posOffset, downRotate) as GameObject;
				newBlock.cells[cellIndexDown].transform.parent = newBlock.block.transform;
				SetCellParams(isUp, col, colInRow, row, cellIndexDown, true, newBlock, endlessBlock);
				posOffset.z = - posOffset.z;

				isUp = !isUp;
				cellIndex++;
			}
		}

		newBlock.block.transform.position = BlockCoordToRealCoord (pos);
		transform.position = backupFieldPos;

		FieldBlock_OptimizeBack (newBlock);
		blocks.Add(BlockCoordToID(pos), newBlock);
		return newBlock;
	}

}
