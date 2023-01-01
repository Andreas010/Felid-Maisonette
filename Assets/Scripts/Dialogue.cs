using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.InputSystem;

enum DialogueOpCode
{
    Terminator = 0,
    Return,
    Speak,
    Jump
}

class DialogueSection
{
    public List<char> code;
    public string name;

    public DialogueSection()
    {
        code = new List<char>();
    }

    public void AddSpeech(string text)
    {
        code.Add((char)DialogueOpCode.Speak);

        foreach (char c in text)
        {
            code.Add(c);
        }

        code.Add((char)DialogueOpCode.Terminator);
    }

    public void AddTerminator()
    {
        code.Add((char)DialogueOpCode.Terminator);
    }

    public void AddReturn()
    {
        code.Add((char)DialogueOpCode.Return);
    }
}

class DialogueInterpreter
{
    public delegate void OnNextFunc(string text);

    Dictionary<string, DialogueSection> sections;
    DialogueSection section;
    OnNextFunc onNext;

    int ip; /* Instruction pointer. */
    bool done;

    public DialogueInterpreter(string src, OnNextFunc f)
    {
        sections = new Dictionary<string, DialogueSection>();
        onNext = f;

        DialogueSection current_section = null;

        int lc = 1;

        using (StringReader reader = new StringReader(src))
        {
            string line;
            string buf = "";
            while ((line = reader.ReadLine()) != null)
            {
                int start = 0;

                while (char.IsWhiteSpace(line[start]))
                {
                    start++;
                }

                switch (line[start])
                {
                    case '&':
                        buf = "";
                        for (int i = start + 1; i < line.Length; i++)
                        {
                            buf += line[i];
                        }

                        if (current_section != null)
                        {
                            current_section.AddTerminator();
                            sections.Add(current_section.name, current_section);
                        }
                        current_section = new DialogueSection();
                        current_section.name = buf;
                        break;
                    default:
                        if (current_section == null)
                        {
                            Debug.LogError("Dialogue, line " + lc.ToString() + "Invalid character.");
                            return;
                        }
                        current_section.AddSpeech(line);
                        break;
                }

                lc++;
            }

            current_section.AddReturn();
            sections.Add(current_section.name, current_section);
        }
    }

    public void Execute(string section_name = "main")
    {
        if (!sections.ContainsKey(section_name))
        {
            Debug.LogError("Section \"" + section_name + "\" doesn't exist.");
            return;
        }

        Execute(sections[section_name]);
    }

    public void Execute(DialogueSection sec)
    {
        section = sec;

        ip = 0;
        done = false;

        Continue();
    }

    void ExecuteSpeak()
    {
        string buf = "";

        for (; section.code[ip] != (char)DialogueOpCode.Terminator; ip++)
        {
            buf += section.code[ip];
        }

        ip++;

        onNext(buf);
    }

    public void Continue()
    {
        char c;

        while (true)
        {
            c = section.code[ip++];

            switch ((DialogueOpCode)c)
            {
                case DialogueOpCode.Speak:
                    ExecuteSpeak();
                    return;
                case DialogueOpCode.Terminator:
                    ip++;
                    return;
                case DialogueOpCode.Return:
                    done = true;
                    return;
                default:
                    Debug.Log(c);
                    Debug.LogError("Invalid instruction.");
                    return;
            }
        }
    }

    public bool Finished()
    {
        return done;
    }
}

class Dialogue : MonoBehaviour
{
    [SerializeField] TextAsset src;
    [SerializeField] TMP_Text typewriterText;

    Controls input;

    DialogueInterpreter engine;

    string currentText;
    int displayLen;

    void Awake()
    {
        input = GameManager.Singleton.Input;
        input.World.Enable();

        input.World.Jump.performed += OnWantNextDialogue;

        engine = new DialogueInterpreter(src.text, OnNextDialogue);

        /* This would obviously be called from an
         * OnTriggerEnter or something. */
        engine.Execute("main");
    }

    void OnWantNextDialogue(InputAction.CallbackContext t_context)
    {
        Next();
    }

    void OnNextDialogue(string text)
    {
        currentText = text;
        displayLen = 0;
    }

    void FixedUpdate()
    {
        if (displayLen < currentText.Length)
        {
            /* Should probably take the timestep
             * into account in case it gets changed. */
            displayLen++;

            typewriterText.text = currentText.Substring(0, displayLen);
        }
    }

    void Next()
    {
        engine.Continue();

        if (engine.Finished())
        {
            /* This effectively loops the dialoge.
             * Just remove the line below to make it not loop. */
            engine.Execute("main");
            return;
        }
    }
}
