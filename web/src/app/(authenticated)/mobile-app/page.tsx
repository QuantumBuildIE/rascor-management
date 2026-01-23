"use client";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import {
  Smartphone,
  Download,
  FolderOpen,
  ShieldCheck,
  Package,
  MapPin,
  Mail,
  Info,
  AlertTriangle,
  HelpCircle,
  CheckCircle2,
  Send,
  ArrowLeft,
} from "lucide-react";

export default function MobileAppPage() {
  return (
    <div className="space-y-8 max-w-4xl mx-auto">
      {/* Header Section */}
      <div>
        <div className="flex items-center gap-3 mb-2">
          <Smartphone className="h-8 w-8 text-primary" />
          <h1 className="text-2xl font-semibold tracking-tight">
            Get the RASCOR Mobile App
          </h1>
        </div>
        <p className="text-muted-foreground">
          Download and install the Geofence app on your Android device
        </p>
      </div>

      {/* Platform Notice */}
      <Alert className="border-blue-200 bg-blue-50 dark:bg-blue-950/30 dark:border-blue-800">
        <Info className="h-4 w-4 text-blue-600 dark:text-blue-400" />
        <AlertTitle className="text-blue-800 dark:text-blue-300">
          Android Only
        </AlertTitle>
        <AlertDescription className="text-blue-700 dark:text-blue-400">
          The RASCOR Geofence app is currently available for Android devices
          only. iOS version coming soon.
        </AlertDescription>
      </Alert>

      {/* Download Section */}
      <Card className="border-2 border-primary/20">
        <CardHeader className="text-center pb-4">
          <CardTitle className="text-xl">Download the App</CardTitle>
          <CardDescription>
            Get the latest version of RASCOR Geofence
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col items-center gap-4">
          <Button
            size="lg"
            className="h-14 px-8 text-lg gap-3"
            asChild
          >
            <a href="https://pub-cb8b7507e2a34ca2a366caa3bca24d08.r2.dev/Downloads/com.yourorg.Rascor-Signed.apk" download="RASCOR-Geofence.apk">
              <Download className="h-6 w-6" />
              Download APK
            </a>
          </Button>
          <div className="text-center space-y-1">
            <p className="font-medium">RASCOR-Geofence.apk</p>
            <p className="text-sm text-muted-foreground">
              Version 1.0.0 &bull; Requires Android 8.0 or higher
            </p>
            <p className="text-sm text-muted-foreground">File size: ~15 MB</p>
          </div>
        </CardContent>
      </Card>

      {/* Installation Steps */}
      <div className="space-y-4">
        <h2 className="text-xl font-semibold">Step-by-Step Installation Guide</h2>

        <div className="grid gap-4">
          {/* Step 1 */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">1</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <Download className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Download the App</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap the green &quot;Download APK&quot; button above
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      If your browser shows a warning, tap &quot;Download anyway&quot; - the file is safe
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Wait for the download to complete (check the notification bar)
                    </li>
                  </ul>
                  <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg">
                    <Info className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <p className="text-sm">
                      <span className="font-medium">Tip:</span> You&apos;ll see a download icon in your notification bar when complete
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 2 */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">2</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <FolderOpen className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Find the Downloaded File</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Open your phone&apos;s &quot;Files&quot; app (or &quot;My Files&quot; on Samsung)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap on &quot;Downloads&quot; folder
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Look for &quot;RASCOR-Geofence.apk&quot;
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap on the file to start installation
                    </li>
                  </ul>
                  <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg">
                    <Info className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <p className="text-sm">
                      <span className="font-medium">Tip:</span> You can also tap the download notification directly when it finishes
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 3 */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">3</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <ShieldCheck className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Allow Installation Permission</h3>
                  </div>

                  <Alert className="border-amber-200 bg-amber-50 dark:bg-amber-950/30 dark:border-amber-800">
                    <AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
                    <AlertTitle className="text-amber-800 dark:text-amber-300">
                      Important Step
                    </AlertTitle>
                    <AlertDescription className="text-amber-700 dark:text-amber-400">
                      Android requires you to grant permission before installing apps downloaded from the internet. This is a security feature and only needs to be done once.
                    </AlertDescription>
                  </Alert>

                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      When you tap the APK file, a popup will appear
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap &quot;Settings&quot; on the popup
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Find the toggle for &quot;Allow from this source&quot; and turn it <strong>ON</strong>
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap the back arrow (&larr;) to return to the installation screen
                    </li>
                  </ul>

                  <div className="p-4 border rounded-lg bg-muted/30 space-y-3">
                    <p className="font-medium text-sm">Can&apos;t find the setting?</p>
                    <ul className="space-y-2 text-sm text-muted-foreground">
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-1">&bull;</span>
                        Go to <strong>Settings</strong> &rarr; <strong>Apps</strong> &rarr; <strong>Special app access</strong> &rarr; <strong>Install unknown apps</strong>
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-1">&bull;</span>
                        Find your browser (Chrome) or Files app in the list
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-1">&bull;</span>
                        Turn <strong>ON</strong> &quot;Allow from this source&quot;
                      </li>
                      <li className="flex items-start gap-2">
                        <span className="text-primary mt-1">&bull;</span>
                        Go back to Downloads and tap the APK file again
                      </li>
                    </ul>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 4 */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">4</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <Package className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Install the App</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap &quot;Install&quot; on the installation screen
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Wait 10-30 seconds for installation to complete
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      When finished, tap &quot;Open&quot; to launch the app (or &quot;Done&quot; to install later)
                    </li>
                  </ul>
                  <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg">
                    <Info className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <p className="text-sm">
                      <span className="font-medium">Note:</span> You&apos;ll find the RASCOR app icon in your app drawer after installation
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 5 - Device Registration */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">5</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <Smartphone className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Automatic Device Registration</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      When the app opens, you&apos;ll see a <strong>&quot;Registration&quot;</strong> page
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      The app will automatically register your device with the server
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Wait a few seconds - you&apos;ll see a loading indicator
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Once complete, your <strong>Device ID</strong> will appear (e.g., <code className="bg-muted px-1.5 py-0.5 rounded text-xs">EVT0001</code>)
                    </li>
                  </ul>
                  <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg">
                    <Info className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <p className="text-sm">
                      <span className="font-medium">Note:</span> The Device ID (EVT followed by numbers) is your unique device identifier. You&apos;ll need this for the next step.
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 6 - Send Registration Email */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">6</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <Mail className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Send Registration Email</h3>
                  </div>

                  <Alert className="border-amber-200 bg-amber-50 dark:bg-amber-950/30 dark:border-amber-800">
                    <AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
                    <AlertTitle className="text-amber-800 dark:text-amber-300">
                      Important - Use Your Work Email
                    </AlertTitle>
                    <AlertDescription className="text-amber-700 dark:text-amber-400">
                      Your email address is used to identify you in the system. Make sure you send the registration email from your work/company email account.
                    </AlertDescription>
                  </Alert>

                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap the green <strong>&quot;Send Registration Email&quot;</strong> button
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Your email app will open automatically with a pre-formatted email
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      <strong>DO NOT modify the email</strong> - it&apos;s already formatted correctly
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Simply tap <strong>&quot;Send&quot;</strong> in your email app
                    </li>
                  </ul>

                  <div className="p-4 border rounded-lg bg-muted/30 space-y-2">
                    <p className="font-medium text-sm">Email Details:</p>
                    <ul className="space-y-1 text-sm text-muted-foreground">
                      <li><strong>To:</strong> <code className="bg-muted px-1.5 py-0.5 rounded text-xs">devicereg@rascor.zohocreatorapp.eu</code></li>
                      <li><strong>Subject:</strong> Your Device ID (e.g., EVT0001)</li>
                      <li><strong>From:</strong> Your email address (used for identification)</li>
                    </ul>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 7 - Confirm Email and Grant Permissions */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">7</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <Send className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Confirm Email Sent</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      After sending the email, return to the RASCOR app
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      The button will now be blue and say <strong>&quot;I&apos;ve Sent the Email&quot;</strong>
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      <strong>Tap the button</strong> to confirm and continue
                    </li>
                  </ul>
                  <div className="flex items-start gap-2 p-3 bg-muted/50 rounded-lg">
                    <ArrowLeft className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                    <p className="text-sm">
                      <span className="font-medium">How to return:</span> Use your phone&apos;s back button or app switcher to return to RASCOR after sending the email
                    </p>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 8 - Grant Location Permission */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-primary/10 flex items-center justify-center">
                    <span className="text-2xl font-bold text-primary">8</span>
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <MapPin className="h-5 w-5 text-primary" />
                    <h3 className="font-semibold text-lg">Grant Location Permission</h3>
                  </div>

                  <Alert className="border-amber-200 bg-amber-50 dark:bg-amber-950/30 dark:border-amber-800">
                    <AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400" />
                    <AlertTitle className="text-amber-800 dark:text-amber-300">
                      Critical - Select &quot;Allow all the time&quot;
                    </AlertTitle>
                    <AlertDescription className="text-amber-700 dark:text-amber-400">
                      The app needs background location access to automatically track when you arrive at and leave job sites. Selecting &quot;Only while using the app&quot; will not work correctly.
                    </AlertDescription>
                  </Alert>

                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      After confirming your email, a location permission popup will appear
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Select <strong>&quot;Allow all the time&quot;</strong>
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      This is required for geofencing to work in the background
                    </li>
                  </ul>

                  <Alert className="border-blue-200 bg-blue-50 dark:bg-blue-950/30 dark:border-blue-800">
                    <Info className="h-4 w-4 text-blue-600 dark:text-blue-400" />
                    <AlertTitle className="text-blue-800 dark:text-blue-300">
                      Why does the app need location &quot;all the time&quot;?
                    </AlertTitle>
                    <AlertDescription className="text-blue-700 dark:text-blue-400">
                      The geofence feature automatically records when you arrive at and leave work sites, even when the app isn&apos;t open. This requires &quot;Allow all the time&quot; permission to function correctly.
                    </AlertDescription>
                  </Alert>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Step 9 - You're All Set */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex gap-4">
                <div className="flex-shrink-0">
                  <div className="w-12 h-12 rounded-full bg-green-100 dark:bg-green-900/30 flex items-center justify-center">
                    <CheckCircle2 className="h-6 w-6 text-green-600 dark:text-green-400" />
                  </div>
                </div>
                <div className="flex-1 space-y-3">
                  <div className="flex items-center gap-2">
                    <h3 className="font-semibold text-lg">You&apos;re All Set!</h3>
                  </div>
                  <ul className="space-y-2 text-sm text-muted-foreground">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      After granting permission, you may see a <strong>&quot;Welcome to RASCOR&quot;</strong> screen
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap <strong>&quot;Get Started&quot;</strong> to continue to the app
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      The app will navigate to the <strong>Home</strong> page
                    </li>
                  </ul>

                  <Alert className="border-green-200 bg-green-50 dark:bg-green-950/30 dark:border-green-800">
                    <CheckCircle2 className="h-4 w-4 text-green-600 dark:text-green-400" />
                    <AlertTitle className="text-green-800 dark:text-green-300">
                      Registration Complete!
                    </AlertTitle>
                    <AlertDescription className="text-green-700 dark:text-green-400">
                      The app runs in the background and will automatically track your site attendance. You don&apos;t need to open it every day. Registration is one-time only.
                    </AlertDescription>
                  </Alert>

                  <div className="p-4 border rounded-lg bg-muted/30 space-y-2">
                    <p className="font-medium text-sm">App Features:</p>
                    <ul className="space-y-1 text-sm text-muted-foreground">
                      <li><strong>Home tab:</strong> View your location status, check-in/check-out of sites</li>
                      <li><strong>History tab:</strong> View past geofence events</li>
                      <li><strong>Settings tab:</strong> View device info, geofence status, start/stop monitoring</li>
                    </ul>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>

      {/* Troubleshooting Section */}
      <div className="space-y-4">
        <div className="flex items-center gap-2">
          <HelpCircle className="h-5 w-5 text-muted-foreground" />
          <h2 className="text-xl font-semibold">Having Problems? Check these common issues</h2>
        </div>

        <Card>
          <CardContent className="pt-6">
            <Accordion type="single" collapsible className="w-full">
              <AccordionItem value="cant-find-file">
                <AccordionTrigger className="text-left">
                  I can&apos;t find the downloaded file
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Check your browser&apos;s downloads: tap the three-dot menu (&vellip;) &rarr; Downloads
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Open the Files app and look in the &quot;Downloads&quot; folder
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Some phones save to &quot;Internal storage&quot; &rarr; &quot;Download&quot;
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Try downloading the file again
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="installation-blocked">
                <AccordionTrigger className="text-left">
                  It says &quot;App not installed&quot; or installation blocked
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure you completed Step 3 (Allow installation permission)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Check you have at least 100MB of free storage space
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      If you had an older version, uninstall it first, then try again
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Download the APK file again - it may have been corrupted during download
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="no-allow-option">
                <AccordionTrigger className="text-left">
                  I don&apos;t see the &quot;Allow from this source&quot; option
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <p className="mb-2">The exact location varies by phone brand:</p>
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      <strong>Samsung:</strong> Settings &rarr; Biometrics and security &rarr; Install unknown apps
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      <strong>Pixel/Stock Android:</strong> Settings &rarr; Apps &rarr; Special app access &rarr; Install unknown apps
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      <strong>Xiaomi:</strong> Settings &rarr; Privacy &rarr; Special permissions &rarr; Install unknown apps
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Look for your browser (Chrome) or Files app in the list
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="email-not-open">
                <AccordionTrigger className="text-left">
                  Email app doesn&apos;t open when I tap the button
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure you have an email app installed (Gmail, Outlook, etc.)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Try installing Gmail from the Google Play Store if you don&apos;t have one
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure you&apos;ve signed into your email app with your work account
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="registration-failed">
                <AccordionTrigger className="text-left">
                  Registration failed or I&apos;m stuck on the registration screen
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Check your internet connection (Wi-Fi or mobile data)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Tap &quot;Retry Registration&quot; if you see an error message
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure you actually <strong>sent</strong> the email (not just opened it)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Return to the app and tap <strong>&quot;I&apos;ve Sent the Email&quot;</strong> button
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Close and reopen the app, then try the registration again
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="not-tracking">
                <AccordionTrigger className="text-left">
                  The app isn&apos;t tracking my location / Geofences not working
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Go to phone Settings &rarr; Apps &rarr; RASCOR &rarr; Permissions &rarr; Location
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure Location is set to <strong>&quot;Allow all the time&quot;</strong> (not &quot;Only while using&quot;)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Make sure your phone&apos;s Location/GPS is turned on
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Open the app, go to <strong>Settings</strong> tab, and tap <strong>&quot;Start Monitoring&quot;</strong>
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Use the <strong>&quot;Check Current Location&quot;</strong> button in Settings for diagnostics
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>

              <AccordionItem value="find-device-id">
                <AccordionTrigger className="text-left">
                  Where can I find my Device ID?
                </AccordionTrigger>
                <AccordionContent className="space-y-2 text-muted-foreground">
                  <ul className="space-y-2">
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Open the RASCOR app
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Go to the <strong>Settings</strong> tab
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      Your Device ID is shown under &quot;User Information&quot; (e.g., EVT0001)
                    </li>
                    <li className="flex items-start gap-2">
                      <span className="text-primary mt-1">&bull;</span>
                      You can tap <strong>&quot;Copy Device ID&quot;</strong> to copy it to your clipboard
                    </li>
                  </ul>
                </AccordionContent>
              </AccordionItem>
            </Accordion>
          </CardContent>
        </Card>
      </div>

      {/* Need Help Section */}
      <Card className="bg-muted/30">
        <CardContent className="py-6">
          <div className="flex items-start gap-4">
            <HelpCircle className="h-6 w-6 text-muted-foreground flex-shrink-0" />
            <div>
              <h3 className="font-semibold mb-1">Still having trouble?</h3>
              <p className="text-sm text-muted-foreground">
                Contact your site administrator or IT support for assistance.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
