# 💻 TINY Language Compiler

Welcome to the **TINY Language Compiler** project! This project implements a compiler for the **TINY programming language**, focusing on the first two phases of compilation: ***scanning*** (lexical analysis) and ***parsing*** (syntax analysis).

The compiler is built using **C#** and includes a user-friendly interface for inputting code, displaying errors, and visualizing the results of the scanning and parsing phases.

---

## 🧩 Project Overview

The project consists of three main components:

1. **Main Compiler (C# Implementation)**  
   The core of the project, implementing the **scanner** and **parser** for the TINY language.

2. **Documentation**  
   A document containing:
   - Regular expressions defining the lexical structure of TINY.
   - Deterministic Finite Automata (DFAs) for token classes.

3. **TINY CFG document**  
   A document containing the **Context-Free Grammar (CFG)** for TINY, including:
   - Production rules: Grammar rules with left recursion and left factoring eliminated.
   - Terminals: A list of all terminal symbols.

---

## ✨ Features Implemented

### 1. Scanner (Lexical Analyzer)
- The first part of the TINY compiler. It reads the input code and breaks it into smaller parts called **tokens**, like keywords, numbers, and symbols.

- **Output**:
  - **Errors List**: Displays lexical errors encountered during scanning.
  - **Lexeme-to-Token Table**: A table mapping lexemes to their corresponding tokens.


### 2. Parser (Syntax Analyzer)
- The second phase. It takes the tokens from the scanner and checks whether they follow the rules of the TINY language. It ensures the code makes sense structurally.

- **Output**:
  - **Errors List**: Displays errors from both the scanning and parsing phases.
  - **Lexeme-to-Token Table**: The table produced during the scanning phase.
  - **Parse Tree**: A TreeView component displaying the parse tree generated during parsing using CFG rules.


### 3. Features of the GUI:
  - **Text Area**: For typing or pasting TINY code.
  - **Errors List**: Displays errors detected during scanning and parsing phases.
  - **Lexeme-to-Token Table**: Shows the tokens identified by the scanner.
  - **TreeView**: Displays the generated parse tree.


### 4. Documentation
- **Regular Expressions**: All regular expressions for TINY's lexical structure.
- **DFAs**: Deterministic Finite Automata for token classes.
- **CFG**: Context-Free Grammar for TINY, including terminals and production rules.
