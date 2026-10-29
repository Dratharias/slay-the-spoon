# Copies StS2's card text (eng, fra) out of SlayTheSpire2.pck.
# Run headless with Godot 4.5.1: env PCK=<SlayTheSpire2.pck> OUT=<dir> godot --headless -s dump_sts2_loc.gd
extends SceneTree

func _init():
	if not ProjectSettings.load_resource_pack(OS.get_environment("PCK")):
		push_error("cannot load " + OS.get_environment("PCK"))
		quit(1)
		return
	var out := OS.get_environment("OUT")
	for lang in ["eng", "fra"]:
		var src := "res://localization/%s/cards.json" % lang
		var text := FileAccess.get_file_as_string(src)
		if text.is_empty():
			push_error("missing " + src)
			quit(1)
			return
		DirAccess.make_dir_recursive_absolute(out + "/" + lang)
		var f := FileAccess.open(out + "/" + lang + "/cards.json", FileAccess.WRITE)
		f.store_string(text)
		f.close()
	quit()
