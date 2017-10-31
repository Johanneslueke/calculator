(defun make-list (x) 
	/* returns x if it is a list, otherwise (x . nil) */
	(cond ((nil-p x) x)
		  ((list-p x) x)
		  (t (cons x nil))))

(defun append (l x)
	/* appends x to list l */
	(cond ((nil-p l) (make-list x))
		  ((atom-p l) (cons l x))
		  (t (cons (car l) (append (cdr l) x)))))

(defun reverse (l)
	/* reverses the list */
	(cond ((nil-p l) l)
		  ((atom-p l) (cons l nil))
		  (t (append (reverse (cdr l)) (car l)))))

