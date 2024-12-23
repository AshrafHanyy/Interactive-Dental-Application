import tkinter as tk
from tkinter import messagebox, ttk

class CustomButton(tk.Button):
    def __init__(self, master=None, **kwargs):
        super().__init__(master, 
                        bg='#2c4157',          # Button background
                        fg='#ffffff',          # Text color
                        activebackground='#3c5167',  # Hover background
                        activeforeground='#ffffff',  # Hover text color
                        font=('Arial', 12),
                        relief='raised',
                        borderwidth=2,
                        cursor='hand2',
                        padx=20,
                        pady=10,
                        **kwargs)

class QuizApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("MCQ Quiz Application")
        self.root.geometry("800x600")
        
        # Define colors
        self.colors = {
            'bg': '#1a2a3a',    # Dark navy
            'fg': '#ffffff',    # White text
            'button': '#2c4157' # Lighter navy for buttons
        }
        
        # Configure root window
        self.root.configure(bg=self.colors['bg'])
        
        # Configure style for other elements
        self.style = ttk.Style()
        self.style.configure('Custom.TFrame', background=self.colors['bg'])
        self.style.configure('Custom.TLabel', 
                           background=self.colors['bg'], 
                           foreground=self.colors['fg'],
                           font=('Arial', 12))
        self.style.configure('Title.TLabel',
                           background=self.colors['bg'],
                           foreground=self.colors['fg'],
                           font=('Arial', 18, 'bold'))
        self.style.configure('Custom.TRadiobutton',
                           background=self.colors['bg'],
                           foreground=self.colors['fg'],
                           font=('Arial', 12))
        
        # Dummy users (username: password)
        self.users = {
            "admin1": {"password": "admin123", "role": "admin"},
            "user1": {"password": "user123", "role": "student"}
        }
        
        # Dummy questions
        self.questions = {
            1: {
                "question": "What is the capital of France?",
                "choices": ["London", "Berlin", "Paris", "Madrid"],
                "correct": 2
            },
            2: {
                "question": "Which planet is known as the Red Planet?",
                "choices": ["Venus", "Mars", "Jupiter", "Saturn"],
                "correct": 1
            },
            3: {
                "question": "What is 2 + 2?",
                "choices": ["3", "4", "5", "6"],
                "correct": 1
            }
        }
        
        self.current_user = None
        self.current_question = 1
        self.user_answers = {}
        
        self.create_login_page()
        
    def create_login_page(self):
        self.clear_window()
        
        main_frame = ttk.Frame(self.root, style='Custom.TFrame')
        main_frame.pack(expand=True, fill='both', padx=50, pady=50)
        main_frame.grid_columnconfigure(0, weight=1)
        
        # Title
        title_label = ttk.Label(main_frame, text="Quiz Login", style='Title.TLabel')
        title_label.grid(row=0, column=0, pady=(0, 40))
        
        # Login form
        login_frame = ttk.Frame(main_frame, style='Custom.TFrame')
        login_frame.grid(row=1, column=0)
        
        ttk.Label(login_frame, text="Username:", style='Custom.TLabel').grid(row=0, column=0, pady=10)
        username_entry = ttk.Entry(login_frame, width=30, font=('Arial', 12))
        username_entry.grid(row=1, column=0, pady=5)
        
        ttk.Label(login_frame, text="Password:", style='Custom.TLabel').grid(row=2, column=0, pady=10)
        password_entry = ttk.Entry(login_frame, show="*", width=30, font=('Arial', 12))
        password_entry.grid(row=3, column=0, pady=5)
        
        def login():
            username = username_entry.get()
            password = password_entry.get()
            
            if username in self.users and self.users[username]["password"] == password:
                self.current_user = username
                if self.users[username]["role"] == "admin":
                    self.create_admin_page()
                else:
                    self.create_quiz_page()
            else:
                messagebox.showerror("Error", "Invalid credentials")
        
        login_button = CustomButton(login_frame, text="Login", command=login)
        login_button.grid(row=4, column=0, pady=30)
        
    def create_admin_page(self):
        self.clear_window()
        
        main_frame = ttk.Frame(self.root, style='Custom.TFrame')
        main_frame.pack(expand=True, fill='both', padx=50, pady=50)
        main_frame.grid_columnconfigure(0, weight=1)
        
        ttk.Label(main_frame, text="Admin Panel", style='Title.TLabel').grid(row=0, column=0, pady=(0, 40))
        
        def add_question():
            add_window = tk.Toplevel(self.root)
            add_window.title("Add Question")
            add_window.geometry("500x600")
            add_window.configure(bg=self.colors['bg'])
            
            add_frame = ttk.Frame(add_window, style='Custom.TFrame')
            add_frame.pack(expand=True, fill='both', padx=30, pady=30)
            
            ttk.Label(add_frame, text="Add New Question", style='Title.TLabel').pack(pady=(0, 20))
            
            ttk.Label(add_frame, text="Question:", style='Custom.TLabel').pack(pady=5)
            question_entry = ttk.Entry(add_frame, width=50, font=('Arial', 12))
            question_entry.pack(pady=(0, 20))
            
            choices = []
            for i in range(4):
                ttk.Label(add_frame, text=f"Choice {i+1}:", style='Custom.TLabel').pack(pady=5)
                choice_entry = ttk.Entry(add_frame, width=50, font=('Arial', 12))
                choice_entry.pack(pady=(0, 10))
                choices.append(choice_entry)
            
            ttk.Label(add_frame, text="Correct Answer (1-4):", style='Custom.TLabel').pack(pady=5)
            correct_entry = ttk.Entry(add_frame, width=10, font=('Arial', 12))
            correct_entry.pack(pady=(0, 20))
            
            def save_question():
                new_question = {
                    "question": question_entry.get(),
                    "choices": [choice.get() for choice in choices],
                    "correct": int(correct_entry.get()) - 1
                }
                new_id = max(self.questions.keys()) + 1
                self.questions[new_id] = new_question
                messagebox.showinfo("Success", "Question added successfully!")
                add_window.destroy()
            
            CustomButton(add_frame, text="Save Question", command=save_question).pack(pady=20)
        
        CustomButton(main_frame, text="Add Question", command=add_question).grid(row=1, column=0, pady=10)
        CustomButton(main_frame, text="Logout", command=self.create_login_page).grid(row=2, column=0, pady=10)
        
    def create_quiz_page(self):
        self.clear_window()
        
        main_frame = ttk.Frame(self.root, style='Custom.TFrame')
        main_frame.pack(expand=True, fill='both', padx=50, pady=50)
        main_frame.grid_columnconfigure(0, weight=1)
        
        ttk.Label(main_frame, 
                 text=f"Question {self.current_question} of {len(self.questions)}", 
                 style='Custom.TLabel').grid(row=0, column=0, pady=(0, 20))
        
        question_text = ttk.Label(main_frame, 
                                text=self.questions[self.current_question]["question"],
                                style='Title.TLabel',
                                wraplength=500)
        question_text.grid(row=1, column=0, pady=(0, 40))
        
        choices_frame = ttk.Frame(main_frame, style='Custom.TFrame')
        choices_frame.grid(row=2, column=0, pady=(0, 40))
        
        selected_choice = tk.IntVar()
        for i, choice in enumerate(self.questions[self.current_question]["choices"]):
            ttk.Radiobutton(choices_frame, 
                          text=choice, 
                          variable=selected_choice,
                          value=i,
                          style='Custom.TRadiobutton').pack(pady=10)
        
        if self.current_question in self.user_answers:
            selected_choice.set(self.user_answers[self.current_question])
        
        def save_answer():
            self.user_answers[self.current_question] = selected_choice.get()
        
        def next_question():
            save_answer()
            if self.current_question < len(self.questions):
                self.current_question += 1
                self.create_quiz_page()
            else:
                self.show_results()
        
        def prev_question():
            save_answer()
            if self.current_question > 1:
                self.current_question -= 1
                self.create_quiz_page()
        
        nav_frame = ttk.Frame(main_frame, style='Custom.TFrame')
        nav_frame.grid(row=3, column=0, pady=20)
        
        CustomButton(nav_frame, text="Previous", command=prev_question).pack(side='left', padx=10)
        CustomButton(nav_frame, text="Next", command=next_question).pack(side='left', padx=10)
        
        if self.current_question == len(self.questions):
            CustomButton(main_frame, text="Submit Quiz",
                        command=self.show_results).grid(row=4, column=0, pady=20)
    
    def show_results(self):
        self.clear_window()
        
        main_frame = ttk.Frame(self.root, style='Custom.TFrame')
        main_frame.pack(expand=True, fill='both', padx=50, pady=50)
        main_frame.grid_columnconfigure(0, weight=1)
        
        correct_answers = 0
        for q_id, answer in self.user_answers.items():
            if answer == self.questions[q_id]["correct"]:
                correct_answers += 1
        
        score = (correct_answers / len(self.questions)) * 100
        
        ttk.Label(main_frame, text="Quiz Results", style='Title.TLabel').grid(row=0, column=0, pady=(0, 40))
        ttk.Label(main_frame, text=f"Score: {score:.2f}%", style='Custom.TLabel').grid(row=1, column=0, pady=10)
        ttk.Label(main_frame, text=f"Correct Answers: {correct_answers}/{len(self.questions)}", 
                 style='Custom.TLabel').grid(row=2, column=0, pady=10)
        
        CustomButton(main_frame, text="Return to Login",
                    command=self.create_login_page).grid(row=3, column=0, pady=30)
    
    def clear_window(self):
        for widget in self.root.winfo_children():
            widget.destroy()
    
    def run(self):
        self.root.mainloop()

if __name__ == "__main__":
    app = QuizApp()
    app.run()