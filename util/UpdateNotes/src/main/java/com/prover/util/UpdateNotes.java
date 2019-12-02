package com.prover.util;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.nio.file.CopyOption;
import java.nio.file.Files;
import java.nio.file.StandardCopyOption;

/*
 * https://emacs.stackexchange.com/questions/22584/disable-enlarged-org-mode-header-appearance
 * simpler icons
 * 
 * https://github.com/bbatsov/zenburn-emacs
 */

public class UpdateNotes
{
	int[] rgnIndent = new int[80];
	int nTabs = 0;
	String stConverted;
	
    void copyFile(File fileInput, File fileOutput)
    {
        //overwrite existing file, if exists
        CopyOption[] options = new CopyOption[]{
          StandardCopyOption.REPLACE_EXISTING,
          StandardCopyOption.COPY_ATTRIBUTES
        }; 
        try
		{
			Files.copy(fileInput.toPath(), fileOutput.toPath(), options);
		} catch (IOException e)
		{
			System.err.println("could not copy " + fileInput.getAbsolutePath());
			e.printStackTrace();
		}
    }
    
    boolean match (String stR, int nStart, int ...rgnPattern)
    {
    	if (stR.length() < nStart + rgnPattern.length)
    		return false;
    	for (int i = 0; i < rgnPattern.length; i++)
    	{
    		if (stR.charAt(nStart + i) != rgnPattern[i])
				return false;
    	}
    	return true;
    }
    
/*
 * handle
 * 
 * top level being hidden
 * 
 */
    void indentation(String stR) 
    {
    	int p = 0;
    	int nLen = stR.length();
    	for (int i = 0; i < nLen; i++)
    	{
    		char c = stR.charAt(i);
    		if (c == 10 || c == 13)
    		{
    			continue;
    		}
    		if (c == ' ')
    		{
    			p += 1;
    			continue;
    		}
    		if (c == 9)
    		{
    			p += 4;
    			nTabs++;
    			continue;
    		}
    		if (p <= 79)
    			rgnIndent[p] ++;
    		{
    			/*boolean fSkip = false;
    			fSkip |= match(stR, i, 0xe2, 0x80, 0xa2);
    			if (fSkip)
    			{
    				i += 3;
    				p += 4;
    			} */
    			if (c > 128)
    			{
    				i ++;
    				if (i < nLen && stR.charAt(i) == ' ')
    					i++;
    			}
    			
    			StringBuffer sb = new StringBuffer();
    			for (int j = 0; j < p; j += 4)
    				sb.append('*');
   				sb.append("* ");
    			sb.append(stR.substring(i));
    			stConverted = sb.toString();
    		}
    		return;
    	}
    	rgnIndent[0]++;
    	stConverted = "";
    }

    void processFile(File fileInput, File fileOutput)
    {
        FileWriter fw = null;
        BufferedWriter bw = null;
		BufferedReader br = null;	
		FileReader fr = null;	
		try
		{
			fr = new FileReader(fileInput);
			br = new BufferedReader(fr);
			fw = new FileWriter(fileOutput);
			bw = new BufferedWriter(fw);
			bw.write("#+STARTUP: showall\n");
			String stInput;
			while ((stInput = br.readLine()) != null)
			{
				indentation(stInput);
				bw.write(stConverted);
				bw.write('\n');
			}
		}
		catch (FileNotFoundException e)
		{
			System.err.println("could not open " + fileInput.getAbsolutePath());
		}
		catch (IOException e)
		{
			System.err.println("error reading " + fileInput.getAbsolutePath());
			e.printStackTrace();
		}
		finally
		{
			try 
			{
				if (fr != null)
					fr.close();
				if (br != null)
					br.close();
				if (bw != null)
					bw.close();
				if (fw != null)
					fw.close();
			}
			catch (Exception e)
			{
				System.err.println("error closing files for " + fileInput.getAbsolutePath());
			}
		}
	}

	void processDir(File fileInput, File fileOutput)
	{
		fileOutput.mkdir();
		for (File fileSubIn : fileInput.listFiles())
		{
			File fileSubOut = new File(fileOutput, fileSubIn.getName());
			if (fileSubIn.isDirectory())
				processDir(fileSubIn, fileSubOut);
			else if (fileSubIn.getName().endsWith(".png"))
			{
				copyFile(fileSubIn, fileSubOut);
			}
			else
				processFile(fileSubIn, fileSubOut);
		}
	}

	void report()
	{
		for (int i = 0; i < rgnIndent.length; i++)
		{
			if (rgnIndent[i] != 0)
				System.out.println("indent[" + i + "] = " + rgnIndent[i]);
		}
		System.out.println("num tabs=" + nTabs);
	}
	
	public static void main(String[] rgstArgs)
	{
		System.out.println("starting");
		File fileInput = new File("c:\\Users\\user\\projects\\prover\\notes");
		if (!fileInput.isDirectory())
		{
			System.err.println("is not directory:" + fileInput.getAbsolutePath());
			return;
		}
		File fileOut = new File("c:\\Users\\user\\temp\\notes");
		UpdateNotes proc = new UpdateNotes();
		proc.processDir(fileInput, fileOut);
		proc.report();
	}
}
