extends Reference

static func _snake2pascal(string: String) -> String:
	var pascal_string := '';
	var capitalizeNext = true;
	for i in string.length():
		if string[i] == '_':
			if i == 0:
				pascal_string += '_'
			else:
				capitalizeNext = true;
		elif capitalizeNext:
			pascal_string += string[i].capitalize();
			capitalizeNext = false
		else:
			pascal_string += string[i]
	return pascal_string

static func has_interop_method(obj: Object, snake_case_method: String) -> bool:
	return obj.has_method(snake_case_method) or obj.has_method(_snake2pascal(snake_case_method))

static func try_call_interop_method(obj: Object, snake_case_method: String, arg_array: Array = [], default_return = null):
	if obj.has_method(snake_case_method):
		return obj.callv(snake_case_method, arg_array)
	var pascal_case_method = _snake2pascal(snake_case_method);
	if obj.has_method(pascal_case_method):
		return obj.callv(pascal_case_method, arg_array)
	return default_return
