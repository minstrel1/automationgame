extends Node


static var player: Player

static var seconds_per_hour = 90
static var ticks_per_second = 60

var growables = {
	"test_plant" : {
		"time_to_grow" : 1, # in-game hours
		"yield" : {

		}
	}
}

func hours_to_ticks (input: float) -> int:
	return round(input * seconds_per_hour * ticks_per_second)

# Called when the node enters the scene tree for the first time.
func _ready():
	pass


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	pass
