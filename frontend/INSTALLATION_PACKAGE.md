# 📦 Kurdost AI - Unity Package Installation

## ✅ تثبيت كـ Plugin (محترف)

هذا package يتثبت **كـ add-on عام** في أي Unity project

### الطريقة 1: Git URL (الأسهل)

**في أي Unity project:**

1. **Window → Package Manager**
2. اضغط **+** icon
3. اختر **Add package from git URL**
4. اكتب:
   ```
   https://github.com/kurdostkozer-bit/kurdost-ai.git?path=frontend
   ```
5. اضغط **Add**

✅ بيتثبت تلقائياً!

---

### الطريقة 2: Manual Installation

1. Clone repository:
   ```bash
   git clone https://github.com/kurdostkozer-bit/kurdost-ai.git
   ```

2. في Unity Package Manager:
   - اضغط **+**
   - اختر **Add package from disk**
   - اختر المجلد: `frontend/` من المشروع المنسوخ

3. بيتثبت في project

---

### الطريقة 3: استخدام الـ Package Manager Manifest

في project directory، عدّل `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.kurdost.ai": "https://github.com/kurdostkozer-bit/kurdost-ai.git?path=frontend"
  }
}
```

بعدها Unity بتحمّل و تثبت تلقائياً

---

## 🚀 استخدام بعد التثبيت

بعد التثبيت، بيصير available في أي project:

1. **Window → Kurdost AI → Main**
2. بتظهر الـ chat window
3. أدخل Groq API key
4. ابدأ الـ chat!

---

## 📁 Package Structure

```
com.kurdost.ai/
├── Runtime/
│   ├── KurdostAIChatService.cs (الـ core service)
│   ├── KurdostAI.Runtime.asmdef
│   └── ...
│
├── Editor/
│   ├── KurdostAIMainWindow.cs (الـ UI window)
│   ├── KurdostAI.Editor.asmdef
│   └── ...
│
├── package.json (معلومات الـ package)
└── README.md
```

---

## ✨ المزايا

✅ **تثبت مرة واحدة**  
✅ **متاح في جميع projects**  
✅ **سهل التحديث**  
✅ **احترافي و نظيف**  
✅ **يعمل في أي Unity version 2020.3+**

---

## 🔧 Troubleshooting

### المشكلة: "Package not found"
- تأكد من الـ Git URL صحيح
- تأكد من الـ network connection

### المشكلة: "Assembly definition error"
- اعادة تشغيل Unity
- Delete Library folder

### المشكلة: Window ما تظهر
- تأكد من Package مثبت
- **Window menu → اضغط Refresh**

---

**الآن أنت عندك professional Unity plugin!** 🎉
