var TurnX:float;
var TurnY:float;
var TurnZ:float;

var MoveX:float;
var MoveY:float;
var MoveZ:float;

var World:boolean;

function Update() {
	if (World==true){
    	transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.World);
    	transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.World);
    	
    }else{
    	transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.Self);
    	transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.Self);
    }

}